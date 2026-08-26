using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace iTender.Compliance.Infrastructure.Services
{
    public class EscalationService : IEscalationService
    {
        private readonly ICaseLetterRepository _caseLetterRepository;
        private readonly IComplianceCaseRepository _complianceCaseRepository;
        private readonly IAgsaReferralRepository _agsaReferralRepository;
        private readonly IAgentRepository _agentRepository;
        private readonly ICorrespondenceService _correspondenceService;
        private readonly IDocumentService _documentService;
        private readonly ISystemSettingService _systemSettingService;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EscalationService> _logger;

        public EscalationService(
            ICaseLetterRepository caseLetterRepository,
            IComplianceCaseRepository complianceCaseRepository,
            IAgsaReferralRepository agsaReferralRepository,
            IAgentRepository agentRepository,
            ICorrespondenceService correspondenceService,
            IDocumentService documentService,
            ISystemSettingService systemSettingService,
            INotificationService notificationService,
            IAuditService auditService,
            ICurrentUserService currentUser,
            IUnitOfWork unitOfWork,
            ILogger<EscalationService> logger)
        {
            _caseLetterRepository = caseLetterRepository;
            _complianceCaseRepository = complianceCaseRepository;
            _agsaReferralRepository = agsaReferralRepository;
            _agentRepository = agentRepository;
            _correspondenceService = correspondenceService;
            _documentService = documentService;
            _systemSettingService = systemSettingService;
            _notificationService = notificationService;
            _auditService = auditService;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task RunEscalationCycleAsync(CancellationToken cancellationToken = default)
        {
            var toCn = await EscalateOverdueInstructionLettersAsync(cancellationToken);
            var toAgsa = await EscalateOverdueContraventionNoticesAsync(cancellationToken);

            _logger.LogInformation(
                "Escalation cycle complete. {ToCn} case(s) escalated to Contravention Notice, {ToAgsa} referred for enforcement.",
                toCn,
                toAgsa);
        }

        public async Task<int> EscalateOverdueInstructionLettersAsync(CancellationToken cancellationToken = default)
        {
            var settings = await _systemSettingService.GetAsync();

            var outstanding = await _caseLetterRepository.GetOutstandingWithCaseAsync(cancellationToken);

            var candidates = outstanding
                .Where(l => l.Type == LetterType.Instruction || l.Type == LetterType.Reminder)
                .Where(l => l.ComplianceCase.Status == CaseStatus.WaitingForResponse)
                // one CN per case
                .GroupBy(l => l.ComplianceCaseId)
                .Select(g => g.OrderByDescending(x => x.LetterNumber).First())
                .ToList();

            var count = 0;

            foreach (var letter in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var complianceCase = letter.ComplianceCase;
                    var tender = complianceCase.Tender;

                    var dueOn = DateTime.UtcNow.AddDays(settings.ContraventionNoticeResponseDays);

                    await _correspondenceService.SendContraventionNoticeAsync(new SendContraventionNoticeModel
                    {
                        ComplianceCaseId = complianceCase.Id,
                        RecipientName = letter.RecipientName,
                        RecipientEmail = letter.RecipientEmail,
                        TenderNumber = tender.TenderNumber,
                        TenderTitle = tender.Title,
                        EmployerName = tender.EmployerName,
                        ClosingDate = tender.ClosingDate,
                        Reason = "No response received to the Instruction Letter within the required period.",
                        ResponseDueOn = dueOn
                    }, cancellationToken);

                    if (complianceCase.AgentId.HasValue)
                    {
                        await _notificationService.NotifyAsync(new CreateNotificationModel
                        {
                            UserId = complianceCase.AgentId,
                            Title = "Case Escalated to Contravention Notice",
                            Message = $"Tender {tender.TenderNumber} received no response to the Instruction Letter and has been escalated.",
                            Type = NotificationType.Warning,
                            Url = $"/cases/{complianceCase.Id}"
                        }, cancellationToken);
                    }

                    count++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to escalate case letter {CaseLetterId} to a Contravention Notice.",
                        letter.Id);

                    await _auditService.LogAsync(
                        AuditAction.Error,
                        AuditEntity.ComplianceCase,
                        letter.ComplianceCaseId,
                        $"Automatic escalation to Contravention Notice failed: {ex.Message}",
                        _currentUser.UserId,
                        cancellationToken);
                }
            }

            return count;
        }

        public async Task<int> EscalateOverdueContraventionNoticesAsync(CancellationToken cancellationToken = default)
        {
            var outstanding = await _caseLetterRepository.GetOutstandingWithCaseAsync(cancellationToken);

            var candidates = outstanding
                .Where(l => l.Type == LetterType.ContraventionNotice)
                .Where(l => l.ComplianceCase.Status == CaseStatus.ContraventionNoticeIssued)
                .GroupBy(l => l.ComplianceCaseId)
                .Select(g => g.OrderByDescending(x => x.LetterNumber).First())
                .ToList();

            var managers = (await _agentRepository.GetActiveAsync(cancellationToken))
                .Where(a => a.IsManager)
                .ToList();

            var count = 0;

            foreach (var letter in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var complianceCase = letter.ComplianceCase;
                    var tender = complianceCase.Tender;

                    var existing = await _agsaReferralRepository.GetByCaseIdAsync(
                        complianceCase.Id,
                        cancellationToken);

                    if (existing != null)
                        continue;

                    var referralNumber =
                        $"AGSA-{DateTime.UtcNow:yyyyMMdd}-{complianceCase.Id.ToString()[..8].ToUpperInvariant()}";

                    var reason = "No response received to the Contravention Notice within the required period.";

                    var document = await _documentService.GenerateAgsaReferralDocumentAsync(new AgsaReferralDocumentModel
                    {
                        ReferralNumber = referralNumber,
                        TenderNumber = tender.TenderNumber,
                        TenderTitle = tender.Title,
                        EmployerName = tender.EmployerName,
                        Reason = reason,
                        ReferralDate = DateTime.UtcNow
                    }, cancellationToken);

                    var referral = new AGSAReferral
                    {
                        ComplianceCaseId = complianceCase.Id,
                        ReferralNumber = referralNumber,
                        ReferralDate = DateTime.UtcNow,
                        ReferredByUserId = _currentUser.UserId,
                        Reason = reason,
                        Status = EnforcementReferralStatus.Referred,
                        FileName = document.FileName,
                        FilePath = document.FilePath
                    };

                    await _agsaReferralRepository.AddAsync(referral, cancellationToken);

                    complianceCase.Status = CaseStatus.ReferredForEnforcement;
                    complianceCase.ModifiedOn = DateTime.UtcNow;

                    await _complianceCaseRepository.UpdateAsync(complianceCase, cancellationToken);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    await _auditService.LogAsync(
                        AuditAction.ReferredToEnforcement,
                        AuditEntity.ComplianceCase,
                        complianceCase.Id,
                        $"Case referred to AGSA for enforcement. Reference {referralNumber}.",
                        _currentUser.UserId,
                        cancellationToken);

                    foreach (var manager in managers)
                    {
                        await _notificationService.NotifyAsync(new CreateNotificationModel
                        {
                            UserId = manager.Id,
                            Title = "Case Referred for Enforcement",
                            Message = $"Tender {tender.TenderNumber} has been referred to AGSA ({referralNumber}) after no response to the Contravention Notice.",
                            Type = NotificationType.Warning,
                            Url = $"/cases/{complianceCase.Id}"
                        }, cancellationToken);
                    }

                    count++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to refer case letter {CaseLetterId} for enforcement.",
                        letter.Id);

                    await _auditService.LogAsync(
                        AuditAction.Error,
                        AuditEntity.ComplianceCase,
                        letter.ComplianceCaseId,
                        $"Automatic referral for enforcement failed: {ex.Message}",
                        _currentUser.UserId,
                        cancellationToken);
                }
            }

            return count;
        }
    }
}