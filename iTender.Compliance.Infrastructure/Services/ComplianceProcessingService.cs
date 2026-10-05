using iTender.Application.DTOs;
using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace iTender.Compliance.Infrastructure.Services
{
    public class ComplianceProcessingService : IComplianceProcessingService
    {
        private readonly IClassOfWorksResolver _classOfWorksResolver;
        private readonly ITenderRepository _tenderRepository;
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
        private readonly ICorrespondenceService _correspondenceService;
        private readonly IWorkingDayCalculator _workingDayCalculator;

        public ComplianceProcessingService(
            IComplianceCaseRepository caseRepository,
            ICorrespondenceService correspondenceService,
            IWorkingDayCalculator workingDayCalculator,
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
            _correspondenceService = correspondenceService;
            _workingDayCalculator = workingDayCalculator;
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
                _logger.LogInformation(
                    "Tender {TenderNumber} is not construction, skipping compliance processing.",
                    tender.TenderNumber);
                return null;
            }

            // 2. Determine tender status (Open / Closed)
            var tenderStatus = tender.ClosingDate > DateTime.UtcNow
                ? TenderStatus.Open
                : TenderStatus.Closed;

            // 3. Check if a ComplianceCase already exists for this tender
            var existingCase = await _caseRepository
                .GetByTenderIdAsync(tender.Id, cancellationToken);

            if (existingCase != null)
            {
                _logger.LogInformation(
                    "Compliance case already exists for tender {TenderNumber}, skipping.",
                    tender.TenderNumber);
                return existingCase.Id;
            }

            // 4. Collect findings (Streams 1 and 2 only - Stream 3 is handled by IVerificationService)
            var findings = new List<ComplianceFinding>();

            // ---- STREAM 1: Class of Works ----
            // OCDS/eTenders never carries a CIDB-specific class-of-works code directly - only a
            // generic works/goods/services category, already used upstream to filter to construction
            // tenders in the first place. Try to resolve a real class from the advert's own text
            // against CIDB's class list first.
            if (string.IsNullOrWhiteSpace(tender.ClassOfWorks))
            {
                tender.ClassOfWorks = await _classOfWorksResolver
                    .ResolveAsync(tender, cancellationToken);

                if (!string.IsNullOrWhiteSpace(tender.ClassOfWorks))
                {
                    // Persist the resolved class onto the Tender record itself so future passes
                    // (and anyone looking at the tender directly) see it without re-resolving.
                    await _tenderRepository.UpdateAsync(tender, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }

            if (string.IsNullOrWhiteSpace(tender.ClassOfWorks))
            {
                findings.Add(new ComplianceFinding
                {
                    Stream = ComplianceStream.ClassOfWorks,
                    FindingType = ComplianceFindingType.IncorrectClassOfWorks,
                    Description = "No class of works specified in the tender advertisement, and none could be inferred from the advert text.",
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

            // ---- STREAM 3: RoP / Award registration ----
            // Removed from inline. Handled by IVerificationService below so it can
            // record VerificationStatus and route failed checks to a CO for review (D03).

            // 5. If no findings, return null (no case created)
            if (!findings.Any())
            {
                _logger.LogInformation(
                    "No compliance findings for tender {TenderNumber}.",
                    tender.TenderNumber);
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

            
            // =========================================================
            // 8b. D07: issue the opening letter for this case
            // =========================================================
            var settings = await _settingRepository.GetAsync(cancellationToken);

            var dueOn = await _workingDayCalculator.AddWorkingDaysAsync(
                DateTime.UtcNow,
                tenderStatus == TenderStatus.Open
                    ? settings.InstructionLetterResponseWorkingDays
                    : settings.ContraventionNoticeResponseDays,
                cancellationToken);

            if (tenderStatus == TenderStatus.Open)
            {
                await _correspondenceService.SendInstructionLetterAsync(new SendInstructionLetterModel
                {
                    ComplianceCaseId = complianceCase.Id,
                    RecipientName = tender.EmployerName,
                    RecipientEmail = tender.ContactEmail ?? string.Empty,
                    TenderNumber = tender.TenderNumber,
                    TenderTitle = tender.Title,
                    EmployerName = tender.EmployerName,
                    ClosingDate = tender.ClosingDate,
                    ResponseDueOn = dueOn
                }, cancellationToken);
            }
            else
            {
                await _correspondenceService.SendContraventionNoticeAsync(new SendContraventionNoticeModel
                {
                    ComplianceCaseId = complianceCase.Id,
                    RecipientName = tender.EmployerName,
                    RecipientEmail = tender.ContactEmail ?? string.Empty,
                    TenderNumber = tender.TenderNumber,
                    TenderTitle = tender.Title,
                    EmployerName = tender.EmployerName,
                    ClosingDate = tender.ClosingDate,
                    Reason = "Award not registered on the Register of Projects.",
                    ResponseDueOn = dueOn
                }, cancellationToken);
            }

            // 9. Audit log
            await _auditService.LogAsync(
                AuditAction.Created,
                AuditEntity.ComplianceCase,
                complianceCase.Id,
                $"Compliance case created with {findings.Count} finding(s).",
                userId,
                cancellationToken);

            _logger.LogInformation(
                "Compliance case {CaseId} created for tender {TenderNumber}.",
                complianceCase.Id,
                tender.TenderNumber);

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