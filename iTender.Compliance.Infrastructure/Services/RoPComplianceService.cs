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
        private readonly IComplianceActionRepository _actionRepository;
        private readonly ICaseLetterRepository _letterRepository;
        private readonly ISystemSettingRepository _settingRepository;
        private readonly IAuditService _auditService;
        private readonly ILetterNumberGenerator _letterNumberGenerator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RoPComplianceService> _logger;

        public RoPComplianceService(
            ITenderRepository tenderRepository,
            IComplianceCaseRepository caseRepository,
            IComplianceFindingRepository findingRepository,
            IComplianceActionRepository actionRepository,
            ICaseLetterRepository letterRepository,
            ISystemSettingRepository settingRepository,
            IAuditService auditService,
            ILetterNumberGenerator letterNumberGenerator,
            IUnitOfWork unitOfWork,
            ILogger<RoPComplianceService> logger)
        {
            _tenderRepository = tenderRepository;
            _caseRepository = caseRepository;
            _findingRepository = findingRepository;
            _actionRepository = actionRepository;
            _letterRepository = letterRepository;
            _settingRepository = settingRepository;
            _auditService = auditService;
            _letterNumberGenerator = letterNumberGenerator;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ProcessUnregisteredAwardsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting RoP compliance check for unregistered awards.");

            // 1. Get all tenders that have been awarded but not registered on RoP.
            var tenders = await _tenderRepository.GetUnregisteredAwardedTendersAsync(cancellationToken);
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
            // Check if a case already exists for this tender (for RoP)
            var existingCase = await _caseRepository.GetByTenderIdAsync(tender.Id, cancellationToken);
            if (existingCase != null)
            {
                // If a case already exists, we should check if it has an RoP finding already.
                // If yes, we might skip or update.
                var hasRoPFinding = existingCase.ComplianceFindings.Any(f => f.Stream == ComplianceStream.RopRegistration);
                if (hasRoPFinding)
                {
                    _logger.LogInformation("Case {CaseId} already has RoP finding for tender {TenderNumber}. Skipping.", existingCase.Id, tender.TenderNumber);
                    return;
                }
                // If no RoP finding, we can add one to the existing case.
                // But for simplicity, we'll create a new case? The business process might allow multiple findings per case.
                // Let's create a new finding and append to the existing case.
                await AddRoPFindingToExistingCase(existingCase, tender, cancellationToken);
                return;
            }

            // No case exists – create a new one.
            await CreateNewCaseForRoP(tender, cancellationToken);
        }

        private async Task AddRoPFindingToExistingCase(ComplianceCase existingCase, Tender tender, CancellationToken cancellationToken)
        {
            // Add RoP finding
            var finding = new ComplianceFinding
            {
                ComplianceCaseId = existingCase.Id,
                Stream = ComplianceStream.RopRegistration,
                FindingType = ComplianceFindingType.AwardNotRegisteredOnRop,
                Description = $"Tender awarded on {tender.AwardedDate} but not registered on RoP.",
                RegulatoryReference = "CIDB Regulation 27 / SFU",
                IdentifiedAt = DateTime.UtcNow,
                TenderStatusAtCheck = TenderStatus.Closed, // Awarded means closed
                IsResolved = false
            };
            await _findingRepository.AddAsync(finding);

            // Initiate letter flow (Instructional Letter first, because RoP uses IL first regardless of open/closed)
            await InitiateRoPCorrespondenceAsync(existingCase, finding, tender, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task CreateNewCaseForRoP(Tender tender, CancellationToken cancellationToken)
        {
            // Create case
            var complianceCase = new ComplianceCase
            {
                TenderId = tender.Id,
                Status = CaseStatus.New,
                Priority = CasePriority.Normal,
                CreatedOn = DateTime.UtcNow
            };
            await _caseRepository.AddAsync(complianceCase);
            await _unitOfWork.SaveChangesAsync(cancellationToken); // get Id

            // Create finding
            var finding = new ComplianceFinding
            {
                ComplianceCaseId = complianceCase.Id,
                Stream = ComplianceStream.RopRegistration,
                FindingType = ComplianceFindingType.AwardNotRegisteredOnRop,
                Description = $"Tender awarded on {tender.AwardedDate} but not registered on RoP.",
                RegulatoryReference = "CIDB Regulation 27 / SFU",
                IdentifiedAt = DateTime.UtcNow,
                TenderStatusAtCheck = TenderStatus.Closed, // Awarded = closed
                IsResolved = false
            };
            await _findingRepository.AddAsync(finding);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Initiate letter flow
            await InitiateRoPCorrespondenceAsync(complianceCase, finding, tender, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task InitiateRoPCorrespondenceAsync(ComplianceCase complianceCase, ComplianceFinding finding, Tender tender, CancellationToken cancellationToken)
        {
            var settings = await _settingRepository.GetAsync(cancellationToken);
            if (settings == null)
                throw new InvalidOperationException("System settings not found.");

            // For RoP, always start with Instructional Letter (48h) regardless of open/closed.
            // Note: In the document, Stream 3 uses IL then CN, similar to open tenders.
            var letterType = LetterType.Instruction;
            var dueDate = DateTime.UtcNow.AddHours(settings.OpenTenderResponseHours);
            var actionType = ComplianceActionType.InstructionalLetterSent;
            var newStatus = CaseStatus.WaitingForResponse;

            // Create action
            var action = new ComplianceAction
            {
                ComplianceCaseId = complianceCase.Id,
                ActionType = actionType,
                Status = ComplianceActionStatus.Pending,
                ActionDate = DateTime.UtcNow,
                ResponseDueDate = dueDate,
                Comments = $"Initial RoP Instructional Letter sent.",
                CreatedOn = DateTime.UtcNow
            };
            await _actionRepository.AddAsync(action);

            // Generate letter number
            int letterNumber = await _letterNumberGenerator.GetNextNumberAsync(letterType);

            // Create letter
            var letter = new CaseLetter
            {
                ComplianceCaseId = complianceCase.Id,
                Type = letterType,
                LetterNumber = letterNumber,
                RecipientName = tender.EmployerName,
                RecipientEmail = tender.ContactEmail ?? string.Empty,
                SentOn = DateTime.UtcNow,
                ResponseDueOn = dueDate,
                EmailSent = false,
                FileName = string.Empty,
                FilePath = string.Empty,
                ComplianceFindingId = finding.Id,
                CreatedOn = DateTime.UtcNow
            };
            await _letterRepository.AddAsync(letter);

            // Update case status
            complianceCase.Status = newStatus;
            await _caseRepository.UpdateAsync(complianceCase);

            // Audit
            await _auditService.LogAsync(
                AuditAction.Created,
                AuditEntity.CaseLetter,
                letter.Id,
                $"RoP Instructional Letter #{letterNumber} issued for tender {tender.TenderNumber}. Response due: {dueDate}",
                null);
        }
    }
}
