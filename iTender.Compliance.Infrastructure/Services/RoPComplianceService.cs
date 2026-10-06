using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace iTender.Compliance.Infrastructure.Services
{
    public class RoPComplianceService : IRoPComplianceService
    {
        private readonly ITenderRepository _tenderRepository;
        private readonly IComplianceCaseRepository _caseRepository;
        private readonly IComplianceFindingRepository _findingRepository;
        private readonly ISystemSettingRepository _settingRepository;
        private readonly IWorkingDayCalculator _workingDayCalculator;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICorrespondenceService _correspondenceService;
        private readonly IClientGroupService _clientGroupService;
        private readonly IAssignmentService _assignmentService;
        private readonly ILogger<RoPComplianceService> _logger;

        public RoPComplianceService(
            ITenderRepository tenderRepository,
            IComplianceCaseRepository caseRepository,
            IComplianceFindingRepository findingRepository,
            ISystemSettingRepository settingRepository,
            IWorkingDayCalculator workingDayCalculator,
            IAuditService auditService,
            IUnitOfWork unitOfWork,
            ICorrespondenceService correspondenceService,
            IClientGroupService clientGroupService,
            IAssignmentService assignmentService,
            ILogger<RoPComplianceService> logger)
        {
            _tenderRepository = tenderRepository;
            _caseRepository = caseRepository;
            _findingRepository = findingRepository;
            _settingRepository = settingRepository;
            _workingDayCalculator = workingDayCalculator;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
            _correspondenceService = correspondenceService;
            _clientGroupService = clientGroupService;
            _assignmentService = assignmentService;
            _logger = logger;
        }

        public async Task ProcessUnregisteredAwardsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting RoP compliance check for unregistered awards.");

            var settings = await _settingRepository.GetAsync(cancellationToken);
            if (settings == null)
                throw new InvalidOperationException("System settings not found.");

            // CIDB finalized rule: 90 days from tender closing + 21-day RoP registration grace period.
            var minimumDaysSinceClosing = settings.RopCheckAfterDays + settings.RopRegistrationGraceDays;

            var tenders = await _tenderRepository.GetUnregisteredAwardedTendersAsync(
                minimumDaysSinceClosing,
                cancellationToken);

            if (!tenders.Any())
            {
                _logger.LogInformation("No unregistered awarded tenders found.");
                return;
            }

            _logger.LogInformation("Found {Count} unregistered awarded tenders.", tenders.Count);

            foreach (var tender in tenders)
            {
                try
                {
                    await ProcessTenderAsync(tender, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing tender {TenderNumber}", tender.TenderNumber);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("RoP compliance check completed.");
        }

        private async Task ProcessTenderAsync(Tender tender, CancellationToken cancellationToken)
        {
            var existingCase = await _caseRepository.GetByTenderIdAsync(tender.Id, cancellationToken);

            if (existingCase != null)
            {
                // If a case already exists and already has an RoP finding, skip.
                var hasRoPFinding = existingCase.ComplianceFindings
                    .Any(f => f.Stream == ComplianceStream.RopRegistration);

                if (hasRoPFinding)
                {
                    _logger.LogInformation(
                        "Case {CaseId} already has RoP finding for tender {TenderNumber}. Skipping.",
                        existingCase.Id, tender.TenderNumber);
                    return;
                }

                // Case exists but has no RoP finding yet. Add the finding only -
                // do not issue another letter. The case already has correspondence in flight.
                await AddRoPFindingToExistingCase(existingCase, tender, cancellationToken);
                return;
            }

            await CreateNewCaseForRoP(tender, cancellationToken);
        }

        private async Task AddRoPFindingToExistingCase(
            ComplianceCase existingCase,
            Tender tender,
            CancellationToken cancellationToken)
        {
            var finding = new ComplianceFinding
            {
                ComplianceCaseId = existingCase.Id,
                Stream = ComplianceStream.RopRegistration,
                FindingType = ComplianceFindingType.AwardNotRegisteredOnRop,
                Description = $"Tender awarded on {tender.AwardedDate:dd MMM yyyy} but not registered on RoP.",
                RegulatoryReference = "CIDB Regulation 27 / SFU",
                IdentifiedAt = DateTime.UtcNow,
                TenderStatusAtCheck = TenderStatus.Closed,
                IsResolved = false
            };

            await _findingRepository.AddAsync(finding, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.Updated,
                AuditEntity.ComplianceCase,
                existingCase.Id,
                $"RoP finding added to existing case for tender {tender.TenderNumber}.",
                null,
                cancellationToken);

            _logger.LogInformation(
                "RoP finding added to existing case {CaseId} for tender {TenderNumber}.",
                existingCase.Id, tender.TenderNumber);
        }

        private async Task CreateNewCaseForRoP(Tender tender, CancellationToken cancellationToken)
        {
            // 1. Create the case.
            var complianceCase = new ComplianceCase
            {
                TenderId = tender.Id,
                Status = CaseStatus.New,
                Priority = CasePriority.Normal,
                CreatedOn = DateTime.UtcNow
            };

            await _caseRepository.AddAsync(complianceCase, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 2. Add the RoP finding.
            var finding = new ComplianceFinding
            {
                ComplianceCaseId = complianceCase.Id,
                Stream = ComplianceStream.RopRegistration,
                FindingType = ComplianceFindingType.AwardNotRegisteredOnRop,
                Description = $"Tender awarded on {tender.AwardedDate:dd MMM yyyy} but not registered on RoP.",
                RegulatoryReference = "CIDB Regulation 27 / SFU",
                IdentifiedAt = DateTime.UtcNow,
                TenderStatusAtCheck = TenderStatus.Closed,
                IsResolved = false
            };

            await _findingRepository.AddAsync(finding, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 3. Attach to a draft client group (BR-14).
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

            // 4. Model C assignment.
            if (group.AgentId.HasValue)
            {
                complianceCase.AgentId = group.AgentId;
                complianceCase.AssignedOn = DateTime.UtcNow;
                complianceCase.Status = CaseStatus.Assigned;
                complianceCase.ModifiedOn = DateTime.UtcNow;

                await _caseRepository.UpdateAsync(complianceCase, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "RoP case {CaseId} inherited group owner {AgentId}.",
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
                            "RoP case {CaseId} assigned to {AgentId}; client group owner set.",
                            complianceCase.Id, complianceCase.AgentId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Assignment failed for RoP case {CaseId}. Case will need manual assignment.",
                        complianceCase.Id);
                }
            }

            // 5. Issue the Instructional Letter via CorrespondenceService (PDF + email + numbering).
            var settings = await _settingRepository.GetAsync(cancellationToken);

            var dueOn = await _workingDayCalculator.AddWorkingDaysAsync(
                DateTime.UtcNow,
                settings.InstructionLetterResponseWorkingDays,
                cancellationToken);

            await _correspondenceService.SendInstructionLetterAsync(
                new SendInstructionLetterModel
                {
                    ComplianceCaseId = complianceCase.Id,
                    RecipientName = tender.EmployerName,
                    RecipientEmail = tender.ContactEmail ?? string.Empty,
                    TenderNumber = tender.TenderNumber,
                    TenderTitle = tender.Title,
                    EmployerName = tender.EmployerName,
                    ClosingDate = tender.ClosingDate,
                    ResponseDueOn = dueOn
                },
                cancellationToken);

            await _auditService.LogAsync(
                AuditAction.Created,
                AuditEntity.ComplianceCase,
                complianceCase.Id,
                $"RoP compliance case created for tender {tender.TenderNumber}. " +
                $"Instructional Letter issued, response due {dueOn:dd MMM yyyy}.",
                null,
                cancellationToken);

            _logger.LogInformation(
                "RoP case {CaseId} created and IL issued for tender {TenderNumber}.",
                complianceCase.Id, tender.TenderNumber);
        }
    }
}