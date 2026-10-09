using iTender.Application.DTOs;
using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Domain.Utilities;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace iTender.Compliance.Infrastructure.Services
{
    public class ComplianceProcessingService : IComplianceProcessingService
    {
        private readonly IClassOfWorksResolver _classOfWorksResolver;
        private readonly ICategoryMappingService _categoryMappingService;
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
            ICategoryMappingService categoryMappingService,
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
            ITenderRepository tenderRepository,
            IClassOfWorksResolver classOfWorksResolver,
            ILogger<ComplianceProcessingService> logger)
        {
            _categoryMappingService = categoryMappingService;
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
            _classOfWorksResolver = classOfWorksResolver;
            _tenderRepository = tenderRepository;
        }

        public async Task<Guid?> ProcessTenderAsync(
            Tender tender,
            HashSet<string> iTenderNumbers,
            IReadOnlyDictionary<string, Tender> iTenderLookup,   // NEW: key = normalized employer+title
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
            // Strategy:
            //  1. The category claims a class (or nothing).
            //  2. If it claims a class, verify the advert text supports it via keywords.
            //  3. If supported -> use it. If not -> null it out and raise a finding.
            //  4. If the category claims nothing, fall back to the text resolver.
            var (isConstructionFromCategory, categoryClaim) = await _categoryMappingService
                .MapCategoryAsync(tender.ProcurementCategory, cancellationToken);

            // Sanity: if the category says non-construction but the tender is here, trust the tender flag.
            if (!isConstructionFromCategory && tender.IsConstruction)
            {
                _logger.LogWarning(
                    "Category '{Category}' maps to non-construction but tender {TenderNumber} is flagged construction. " +
                    "Continuing with construction processing.",
                    tender.ProcurementCategory, tender.TenderNumber);
            }

            string? resolvedClass = categoryClaim;
            ClassOfWorksSource source = string.IsNullOrWhiteSpace(categoryClaim)
                ? ClassOfWorksSource.NotSpecified
                : ClassOfWorksSource.InferredByCategory;

            // If the category didn't give a class, try the text resolver.
            if (string.IsNullOrWhiteSpace(resolvedClass))
            {
                resolvedClass = await _classOfWorksResolver
                    .ResolveAsync(tender, cancellationToken);

                if (!string.IsNullOrWhiteSpace(resolvedClass))
                    source = ClassOfWorksSource.InferredByKeywords;
            }

            // If we have a class, verify the advert text supports it.
            if (!string.IsNullOrWhiteSpace(resolvedClass))
            {
                bool supported = await _classOfWorksResolver
                    .DescriptionSupportsClassAsync(tender, resolvedClass, cancellationToken);

                if (!supported)
                {
                    // Category claimed X, description disagrees. Trust neither.
                    tender.ClaimedClassOfWorks = resolvedClass;
                    tender.ClassOfWorks = null;
                    tender.ClassOfWorksSource = ClassOfWorksSource.UnresolvedConflict;

                    findings.Add(new ComplianceFinding
                    {
                        Stream = ComplianceStream.ClassOfWorks,
                        FindingType = ComplianceFindingType.IncorrectClassOfWorks,
                        Description =
                            $"Category '{tender.ProcurementCategory}' implies class '{resolvedClass}', " +
                            $"but the advert text does not contain any keywords supporting that class. " +
                            $"Class of works left unset for CO review.",
                        RegulatoryReference = "CIDB Regulation 25 / SFU",
                        IdentifiedAt = DateTime.UtcNow,
                        TenderStatusAtCheck = tenderStatus,
                        IsResolved = false
                    });

                    await _tenderRepository.UpdateAsync(tender, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    // Description agrees (or class came from the text resolver).
                    tender.ClassOfWorks = resolvedClass;
                    tender.ClassOfWorksSource = source;
                    tender.ClaimedClassOfWorks = null;

                    await _tenderRepository.UpdateAsync(tender, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

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
                            Description =
                                $"Class '{tender.ClassOfWorks}' does not match the project scope. " +
                                $"Category='{tender.ProcurementCategory}'.",
                            RegulatoryReference = "CIDB Regulation 25 / SFU",
                            IdentifiedAt = DateTime.UtcNow,
                            TenderStatusAtCheck = tenderStatus,
                            IsResolved = false
                        });
                    }
                }
            }
            else
            {
                // Nothing to go on: no category claim, no text match.
                tender.ClassOfWorks = null;
                tender.ClassOfWorksSource = ClassOfWorksSource.NotSpecified;

                findings.Add(new ComplianceFinding
                {
                    Stream = ComplianceStream.ClassOfWorks,
                    FindingType = ComplianceFindingType.IncorrectClassOfWorks,
                    Description =
                        $"No class of works could be determined. " +
                        $"Category='{tender.ProcurementCategory}'. " +
                        $"Advert text did not match any CIDB class keywords.",
                    RegulatoryReference = "CIDB Regulation 25 / SFU",
                    IdentifiedAt = DateTime.UtcNow,
                    TenderStatusAtCheck = tenderStatus,
                    IsResolved = false
                });

                await _tenderRepository.UpdateAsync(tender, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // ---- STREAM 2: Advertised on i-Tender ----
            bool isOnITender = iTenderNumbers.Contains(tender.TenderNumber.Trim());

            tender.IsAdvertised = isOnITender;
            tender.NormalizedEmployerName = EmployerNameNormalizer.Normalize(tender.EmployerName);

            await _tenderRepository.UpdateAsync(tender, cancellationToken);

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
            else
            {
                // It IS on i-Tender, but under a different number -> mismatch finding
                var key = TenderMatchKeyBuilder.Build(tender.EmployerName, tender.Title);

                if (iTenderLookup.TryGetValue(key, out var iTenderMatch) &&
                    !string.Equals(iTenderMatch.TenderNumber?.Trim(), tender.TenderNumber?.Trim(),
                                   StringComparison.OrdinalIgnoreCase))
                {
                    findings.Add(new ComplianceFinding
                    {
                        Stream = ComplianceStream.Advertisement,
                        FindingType = ComplianceFindingType.TenderNumberMismatch,
                        Description =
                            $"Same tender advertised under two different numbers: " +
                            $"e-Tender='{tender.TenderNumber}', i-Tender='{iTenderMatch.TenderNumber}'. " +
                            $"Employer='{tender.EmployerName}', Title='{tender.Title}'.",
                        RegulatoryReference = "CIDB Regulation 26 / SFU",
                        IdentifiedAt = DateTime.UtcNow,
                        TenderStatusAtCheck = tenderStatus,
                        IsResolved = false
                    });
                }
            }

            // ---- STREAM 3: RoP / Award registration ----
            // Removed from inline. Handled by IVerificationService below so it can
            // record VerificationStatus and route failed checks to a CO for review (D03).

            // 5. Compute the initial status from the BRS work baskets (BR-8.1).
            //
            //  - No findings             -> Cleared (verified, no issues)
            //  - Findings on advertisement -> NotAdvertised
            //  - Other findings            -> VerificationRequired (CO to review)
            //
            // Priority is derived from findings count so the CO's queue is ordered.
            CaseStatus initialStatus;
            CasePriority priority;

            if (!findings.Any())
            {
                initialStatus = CaseStatus.Cleared;
                priority = CasePriority.Low;
            }
            else if (findings.Any(f => f.Stream == ComplianceStream.Advertisement))
            {
                initialStatus = CaseStatus.NotAdvertised;
                priority = CasePriority.Normal;
            }
            else
            {
                initialStatus = CaseStatus.VerificationRequired;
                priority = CasePriority.Normal;
            }

            _logger.LogInformation(
                "Tender {TenderNumber}: {FindingCount} finding(s); creating case with status {Status}.",
                tender.TenderNumber, findings.Count, initialStatus);

            // 6. Create the ComplianceCase for EVERY construction tender.
            var complianceCase = new ComplianceCase
            {
                TenderId = tender.Id,
                //Tender = tender,
                Status = initialStatus,
                Priority = priority,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _caseRepository.AddAsync(complianceCase, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 7. Link findings (may be zero).
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

                // Only promote to Assigned if the case isn't already in a terminal or cleared state.
                if (complianceCase.Status == CaseStatus.NewlyIdentified
                    || complianceCase.Status == CaseStatus.VerificationRequired
                    || complianceCase.Status == CaseStatus.NotAdvertised
                    || complianceCase.Status == CaseStatus.New)
                {
                    complianceCase.Status = CaseStatus.Assigned;
                }

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
                        "Assignment failed for case {CaseId}. Notifying managers.",
                        complianceCase.Id);

                    await _assignmentService.NotifyManagersManualAssignmentRequiredAsync(
                        complianceCase,
                        $"Assignment threw: {ex.Message}",
                        cancellationToken);
                }
            }

            // If still no agent after all attempts, make sure managers are notified.
            if (!complianceCase.AgentId.HasValue)
            {
                await _assignmentService.NotifyManagersManualAssignmentRequiredAsync(
                    complianceCase,
                    "No agent was assigned automatically or inherited from the client group.",
                    cancellationToken);
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

            var message = findingCount > 0
    ? $"Tender {tender.TenderNumber} — {tender.EmployerName} has been assigned to you. " +
      $"{findingCount} open finding(s) require attention."
    : $"Tender {tender.TenderNumber} — {tender.EmployerName} has been assigned to you. " +
      $"No compliance findings; confirmation required.";

            await _notificationService.NotifyAsync(new CreateNotificationModel
            {
                UserId = coUserId.Value,
                Title = findingCount > 0 ? "New Compliance Case Assigned" : "New Tender Assigned for Review",
                Message = message,
                Type = NotificationType.Information,
                Url = $"/cases/{complianceCase.Id}"
            }, cancellationToken);
        }
    }
}