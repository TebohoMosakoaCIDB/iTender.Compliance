using iTender.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iTender.Compliance.Infrastructure.Services
{
    public class ComplianceProcessingService : IComplianceProcessingService
    {
        private readonly IComplianceCaseRepository _caseRepository;
        private readonly IComplianceFindingRepository _findingRepository;
        private readonly IComplianceActionRepository _actionRepository;
        private readonly ICaseLetterRepository _letterRepository;
        private readonly ISystemSettingRepository _settingRepository;
        private readonly IAuditService _auditService;
        private readonly IWorkClassificationValidator _classValidator;
        private readonly ILetterNumberGenerator _letterNumberGenerator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ComplianceProcessingService> _logger;

        public ComplianceProcessingService(
            IComplianceCaseRepository caseRepository,
            IComplianceFindingRepository findingRepository,
            IComplianceActionRepository actionRepository,
            ICaseLetterRepository letterRepository,
            ISystemSettingRepository settingRepository,
            IAuditService auditService,
            IWorkClassificationValidator classValidator,
            ILetterNumberGenerator letterNumberGenerator,
            IUnitOfWork unitOfWork,
            ILogger<ComplianceProcessingService> logger)
        {
            _caseRepository = caseRepository;
            _findingRepository = findingRepository;
            _actionRepository = actionRepository;
            _letterRepository = letterRepository;
            _settingRepository = settingRepository;
            _auditService = auditService;
            _classValidator = classValidator;
            _letterNumberGenerator = letterNumberGenerator;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid?> ProcessTenderAsync(
            Tender tender,
            HashSet<string> iTenderNumbers,
            List<ContractModel> crmContracts,
            Guid syncId,
            Guid? userId,
            CancellationToken cancellationToken = default)
        {
            // 1. Skip if not construction
            if (!tender.IsConstruction)
            {
                _logger.LogInformation("Tender {TenderNumber} is not construction, skipping compliance processing.", tender.TenderNumber);
                return null;
            }

            // 2. Determine tender status (Open / Closed)
            var tenderStatus = tender.ClosingDate > DateTime.UtcNow
                ? TenderStatus.Open
                : TenderStatus.Closed;

            // 3. Check if a ComplianceCase already exists for this tender
            var existingCase = await _caseRepository.GetByTenderIdAsync(tender.Id, cancellationToken);
            if (existingCase != null)
            {
                _logger.LogInformation("Compliance case already exists for tender {TenderNumber}, skipping.", tender.TenderNumber);
                return existingCase.Id;
            }

            // 4. Collect findings (both streams)
            var findings = new List<ComplianceFinding>();

            // ---- STREAM 1: Class of Works ----
            if (string.IsNullOrWhiteSpace(tender.ClassOfWorks))
            {
                findings.Add(new ComplianceFinding
                {
                    Stream = ComplianceStream.ClassOfWorks,
                    FindingType = ComplianceFindingType.IncorrectClassOfWorks,
                    Description = "No class of works specified in the tender advertisement.",
                    RegulatoryReference = "CIDB Regulation 25 / SFU",
                    IdentifiedAt = DateTime.UtcNow,
                    TenderStatusAtCheck = tenderStatus,
                    IsResolved = false
                });
            }
            else
            {
                bool classIsValid = await _classValidator.ValidateAsync(
                    tender.ClassOfWorks,
                    tender.Description ?? tender.Title,
                    cancellationToken);

                if (!classIsValid)
                {
                    findings.Add(new ComplianceFinding
                    {
                        Stream = ComplianceStream.ClassOfWorks,
                        FindingType = ComplianceFindingType.IncorrectClassOfWorks,
                        Description = $"Tender advert specifies class '{tender.ClassOfWorks}' which is invalid or does not match the project scope.",
                        RegulatoryReference = "CIDB Regulation 25 / SFU",
                        IdentifiedAt = DateTime.UtcNow,
                        TenderStatusAtCheck = tenderStatus,
                        IsResolved = false
                    });
                }
            }

            // ---- STREAM 2: Advertised on i-Tender ----
            bool isOnITender = iTenderNumbers.Contains(tender.TenderNumber.Trim());
            if (!isOnITender)
            {
                findings.Add(new ComplianceFinding
                {
                    Stream = ComplianceStream.Advertisement,
                    FindingType = ComplianceFindingType.AdvertisementNotOnITender,
                    Description = "Tender advertised on e-Tender/client website but not on i-Tender.",
                    RegulatoryReference = "CIDB Regulation 26 / SFU",
                    IdentifiedAt = DateTime.UtcNow,
                    TenderStatusAtCheck = tenderStatus,
                    IsResolved = false
                });
            }

            // ---- STREAM 3: Award / Contract Registration ----

            var matchingContract = FindMatchingContract(
                tender,
                crmContracts);

            if (tender.AwardedDate.HasValue &&
                tender.AwardValue.HasValue &&
                matchingContract == null)
            {
                findings.Add(new ComplianceFinding
                {
                    Stream = ComplianceStream.RopRegistration,
                    FindingType = ComplianceFindingType.AdvertisementNotOnITender,
                    Description =
                        $"Tender '{tender.TenderNumber}' was awarded on " +
                        $"{tender.AwardedDate:dd MMM yyyy} for " +
                        $"R{tender.AwardValue:N2}, but no corresponding " +
                        "contract was found in the CIDB contract register.",
                    RegulatoryReference = "CIDB Act / Regulations / SFU",
                    IdentifiedAt = DateTime.UtcNow,
                    TenderStatusAtCheck = tenderStatus,
                    IsResolved = false
                });
            }

            // 5. If no findings, return null (no case created)
            if (!findings.Any())
            {
                _logger.LogInformation("No compliance findings for tender {TenderNumber}.", tender.TenderNumber);
                return null;
            }

            // 6. Create the ComplianceCase
            var complianceCase = new ComplianceCase
            {
                TenderId = tender.Id,
                Status = CaseStatus.New,
                Priority = CasePriority.Normal,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = userId
            };
            await _caseRepository.AddAsync(complianceCase, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken); // to get Id

            // 7. Link findings to the case and save
            foreach (var finding in findings)
            {
                finding.ComplianceCaseId = complianceCase.Id;
                await _findingRepository.AddAsync(finding, cancellationToken);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 9. Audit log
            await _auditService.LogAsync(
                AuditAction.Created,
                AuditEntity.ComplianceCase,
                complianceCase.Id,
                $"Compliance case created with {findings.Count} finding(s). " +
                $"{(tenderStatus == TenderStatus.Open ? "Instructional Letter" : "Contravention Notice")} issued.",
                userId,
                cancellationToken);

            _logger.LogInformation("Compliance case {CaseId} created for tender {TenderNumber}.",
                complianceCase.Id, tender.TenderNumber);

            return complianceCase.Id;
        }

        private ContractModel? FindMatchingContract(
            Tender tender,
            List<ContractModel> crmContracts)
        {
            // Matching logic will go here once we establish
            // which CRM field corresponds to Tender.TenderNumber.

            return null;
        }

    }
}