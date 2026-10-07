using iTender.Application.DTOs;
using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Domain.Utilities;
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
        private readonly IAgentRepository _agentRepository;
        private readonly ISystemSettingRepository _settingRepository;
        private readonly IAuditService _auditService;
        private readonly IWorkClassificationValidator _classValidator;
        private readonly ILetterNumberGenerator _letterNumberGenerator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ComplianceProcessingService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IWorkingDayCalculator _workingDayCalculator;
        private readonly IClientGroupService _clientGroupService;
        private readonly IAssignmentService _assignmentService;

        public ComplianceProcessingService(
            IComplianceCaseRepository caseRepository,
            INotificationService notificationService,
            IWorkingDayCalculator workingDayCalculator,
            IComplianceFindingRepository findingRepository,
            IComplianceActionRepository actionRepository,
            IAgentRepository agentRepository,
            ISystemSettingRepository settingRepository,
            IAssignmentService assignmentService,
            IAuditService auditService,
            IWorkClassificationValidator classValidator,
            ILetterNumberGenerator letterNumberGenerator,
            IUnitOfWork unitOfWork,
            IClientGroupService clientGroupService,
            ILogger<ComplianceProcessingService> logger)
        {
            _caseRepository = caseRepository;
            _findingRepository = findingRepository;
            _actionRepository = actionRepository;
            _agentRepository = agentRepository;
            _settingRepository = settingRepository;
            _clientGroupService = clientGroupService;
            _assignmentService = assignmentService;
            _auditService = auditService;
            _classValidator = classValidator;
            _letterNumberGenerator = letterNumberGenerator;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _notificationService = notificationService;
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

            tender.IsAdvertised = isOnITender;
            tender.NormalizedEmployerName = EmployerNameNormalizer.Normalize(tender.EmployerName);

            await _tenderRepository.UpdateAsync(tender, cancellationToken);
            // no SaveChanges yet - it'll be persisted by the next SaveChanges call

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
            // 8b. BR-14: attach the case to a draft client group.
            // =========================================================
            var group = await _clientGroupService.GetOrCreateDraftGroupAsync(
                employerName: tender.EmployerName,
                contactName: tender.ContactName,
                contactEmail: tender.ContactEmail,
                ccEmails: null,
                cancellationToken: cancellationToken);

            await _clientGroupService.AddCaseAsync(
                group.Id,
                complianceCase.Id,
                cancellationToken);

            // =========================================================
            // 8c. Assignment:
            // - If the group already has an owner, inherit it.
            // - Otherwise, run normal assignment and adopt the group owner.
            // =========================================================
            if (group.AgentId.HasValue)
            {
                complianceCase.AgentId = group.AgentId;
                complianceCase.AssignedOn = DateTime.UtcNow;
                complianceCase.Status = CaseStatus.Assigned;
                complianceCase.ModifiedOn = DateTime.UtcNow;

                await _caseRepository.UpdateAsync(complianceCase, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Case {CaseId} inherited group owner {AgentId}.",
                    complianceCase.Id, group.AgentId);
            }
            else
            {
                try
                {
                    await _assignmentService.AssignAsync(complianceCase, cancellationToken);

                    if (complianceCase.AgentId.HasValue)
                    {
                        await _clientGroupService.SetOwnerFromCaseAsync(
                            group.Id, complianceCase.Id, cancellationToken);

                        _logger.LogInformation(
                            "Case {CaseId} assigned to {AgentId}; client group owner set.",
                            complianceCase.Id, complianceCase.AgentId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Assignment failed for case {CaseId}. Case will need manual assignment.",
                        complianceCase.Id);
                }
            }

            await NotifyAssignedCoAsync(
                complianceCase,
                tender,
                findings.Count,
                cancellationToken);

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

        private async Task NotifyAssignedCoAsync(ComplianceCase complianceCase, Tender tender, int findingCount, CancellationToken cancellationToken)
        {
            if (!complianceCase.AgentId.HasValue)
                return;

            var coUserId = await _agentRepository
                .GetUserIdByAgentIdAsync(complianceCase.AgentId.Value, cancellationToken);

            if (!coUserId.HasValue)
                return;

            await _notificationService.NotifyAsync(new CreateNotificationModel
            {
                UserId = coUserId.Value,
                Title = "New Compliance Case Assigned",
                Message =
                    $"Tender {tender.TenderNumber} — {tender.EmployerName} has been " +
                    $"assigned to you. {findingCount} open finding(s) require attention.",
                Type = NotificationType.Information,
                Url = $"/cases/{complianceCase.Id}"
            }, cancellationToken);
        }
    }
}