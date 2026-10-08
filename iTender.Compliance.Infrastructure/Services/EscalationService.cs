using DocumentFormat.OpenXml.Spreadsheet;
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
        private readonly IUserService _userService;
        private readonly IComplianceCaseRepository _complianceCaseRepository;
        private readonly IAgsaReferralRepository _agsaReferralRepository;
        private readonly IAgentRepository _agentRepository;
        private readonly ICorrespondenceService _correspondenceService;
        private readonly IDocumentService _documentService;
        private readonly ISystemSettingService _systemSettingService;
        private readonly IWorkingDayCalculator _workingDayCalculator;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EscalationService> _logger;

        public EscalationService(
            IUserService userService,
            ICaseLetterRepository caseLetterRepository,
            IComplianceCaseRepository complianceCaseRepository,
            IAgsaReferralRepository agsaReferralRepository,
            IAgentRepository agentRepository,
            ICorrespondenceService correspondenceService,
            IDocumentService documentService,
            ISystemSettingService systemSettingService,
            IWorkingDayCalculator workingDayCalculator,
            IEmailService emailService,
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
            _workingDayCalculator = workingDayCalculator;
            _emailService = emailService;
            _notificationService = notificationService;
            _auditService = auditService;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
            _userService = userService;
            _logger = logger;
        }

        public async Task RunEscalationCycleAsync(CancellationToken cancellationToken = default)
        {
            var toCn = await EscalateOverdueInstructionLettersAsync(cancellationToken);
            var toAgsaFromCn = await EscalateOverdueContraventionNoticesAsync(cancellationToken);
            var toAgsaFromDeadline = await EscalateStaleCasesToAgsaAsync(cancellationToken);

            _logger.LogInformation(
                "Escalation cycle complete. {ToCn} case(s) escalated to Contravention Notice, " +
                "{ToAgsaFromCn} referred for enforcement after an overdue CN, " +
                "{ToAgsaFromDeadline} referred for enforcement on the 30-day absolute deadline.",
                toCn,
                toAgsaFromCn,
                toAgsaFromDeadline);
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

                    if (!IsStillNonCompliant(complianceCase))
                    {
                        _logger.LogInformation(
                            "Case {CaseId} is no longer non-compliant; skipping IL escalation.",
                            complianceCase.Id);

                        await _auditService.LogAsync(
                            AuditAction.Updated,
                            AuditEntity.ComplianceCase,
                            complianceCase.Id,
                            "BR-12: Contravention Notice not issued - case is no longer non-compliant.",
                            _currentUser.UserId,
                            cancellationToken);

                        continue;
                    }

                    var tender = complianceCase.Tender;

                    var dueOn = await _workingDayCalculator.AddWorkingDaysAsync(
                        DateTime.UtcNow,
                        settings.ContraventionNoticeResponseDays,
                        cancellationToken);

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
                        var coUserId = await _agentRepository
                            .GetUserIdByAgentIdAsync(complianceCase.AgentId.Value, cancellationToken);

                        if (coUserId.HasValue)
                        {
                            await _notificationService.NotifyAsync(new CreateNotificationModel
                            {
                                UserId = coUserId.Value,
                                Title = "Case Escalated to Contravention Notice",
                                Message = $"Tender {tender.TenderNumber} received no response to the Instruction Letter and has been escalated.",
                                Type = NotificationType.Warning,
                                Url = $"/cases/{complianceCase.Id}"
                            }, cancellationToken);
                        }
                    }

                    // Also notify managers that an IL expired without a response.
                    var managerIds = await _userService.GetManagerUserIdsAsync(cancellationToken);

                    foreach (var managerId in managerIds)
                    {
                        await _notificationService.NotifyAsync(new CreateNotificationModel
                        {
                            UserId = managerId,
                            Title = "Instruction Letter Expired",
                            Message =
                                $"Tender {tender.TenderNumber} received no response to the " +
                                $"Instruction Letter by the deadline. A Contravention Notice has " +
                                "been issued.",
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

            var count = 0;

            foreach (var letter in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var complianceCase = letter.ComplianceCase;

                if (!IsStillNonCompliant(complianceCase))
                {
                    _logger.LogInformation(
                        "Case {CaseId} is no longer non-compliant; skipping AGSA referral.",
                        complianceCase.Id);

                    await _auditService.LogAsync(
                        AuditAction.Updated,
                        AuditEntity.ComplianceCase,
                        complianceCase.Id,
                        "BR-12: AGSA referral skipped - case is no longer non-compliant.",
                        _currentUser.UserId,
                        cancellationToken);

                    continue;
                }

                try
                {
                    // -----------------------------------------------------
                    // BR-12 + workflow: notify the CO that the corrective
                    // period has expired and a compliance outcome must be
                    // verified before enforcement referral.
                    // -----------------------------------------------------
                    if (complianceCase.AgentId.HasValue)
                    {
                        var coUserId = await _agentRepository.GetUserIdByAgentIdAsync(complianceCase.AgentId.Value, cancellationToken);

                        if (coUserId.HasValue)
                        {
                            await _notificationService.NotifyAsync(new CreateNotificationModel
                            {
                                UserId = coUserId.Value,
                                Title = "Post-Corrective Verification Required",
                                Message =
                                    $"Tender {complianceCase.Tender.TenderNumber} " +
                                    $"({complianceCase.Tender.EmployerName}) has reached the end of the " +
                                    "Contravention Notice corrective period. Verify the outcome: " +
                                    "Full, Partial or No Compliance.",
                                Type = NotificationType.Warning,
                                Url = $"/cases/{complianceCase.Id}"
                            }, cancellationToken);
                        }
                    }

                    var referred = await CreateAgsaReferralAsync(
                        letter.ComplianceCase,
                        "No response received to the Contravention Notice within the required period.",
                        cancellationToken);

                    if (referred)
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

        /// <summary>CIDB finalized rule: a case must be resolved or referred for enforcement within 30
        /// calendar days of being allocated to a Compliance Officer, regardless of where it currently
        /// sits in the IL/CN cycle. This is a safety net alongside (not a replacement for) the
        /// CN-overdue trigger above - it catches anything that's slipped past 30 days for any reason.</summary>
        public async Task<int> EscalateStaleCasesToAgsaAsync(CancellationToken cancellationToken = default)
        {
            var settings = await _systemSettingService.GetAsync();

            var cutoff = DateTime.UtcNow.AddDays(-settings.AgsaReferralDeadlineDays);

            var staleCases = await _complianceCaseRepository.GetOpenCasesAssignedBeforeAsync(
                cutoff,
                cancellationToken);

            var count = 0;

            foreach (var complianceCase in staleCases)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (!IsStillNonCompliant(complianceCase))
                    {
                        _logger.LogInformation(
                            "Case {CaseId} is no longer non-compliant; skipping 30-day referral.",
                            complianceCase.Id);

                        await _auditService.LogAsync(
                            AuditAction.Updated,
                            AuditEntity.ComplianceCase,
                            complianceCase.Id,
                            "BR-12: 30-day referral skipped - case is no longer non-compliant.",
                            _currentUser.UserId,
                            cancellationToken);

                        continue;
                    }

                    var referred = await CreateAgsaReferralAsync(
                        complianceCase,
                        $"The {settings.AgsaReferralDeadlineDays}-day statutory deadline from allocation to a " +
                        "Compliance Officer has been reached without the matter being resolved.",
                        cancellationToken);

                    if (referred)
                        count++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to refer stale case {ComplianceCaseId} for enforcement on the 30-day deadline.",
                        complianceCase.Id);

                    await _auditService.LogAsync(
                        AuditAction.Error,
                        AuditEntity.ComplianceCase,
                        complianceCase.Id,
                        $"Automatic 30-day referral for enforcement failed: {ex.Message}",
                        _currentUser.UserId,
                        cancellationToken);
                }
            }

            return count;
        }

        /// <summary>Creates the AGSA referral record and document, moves the case to ReferredForEnforcement,
        /// notifies managers, and emails the referral to AGSA (with the CIDB enforcement unit cc'd). Returns
        /// false without doing anything if the case already has a referral, so both trigger paths can safely
        /// call this without double-referring the same case.</summary>
        private async Task<bool> CreateAgsaReferralAsync(ComplianceCase complianceCase, string reason, CancellationToken cancellationToken)
        {
            var existing = await _agsaReferralRepository.GetByCaseIdAsync(
                complianceCase.Id,
                cancellationToken);

            if (existing != null)
                return false;

            var settings = await _systemSettingService.GetAsync();
            var tender = complianceCase.Tender;

            var referralNumber =
                $"AGSA-{DateTime.UtcNow:yyyyMMdd}-{complianceCase.Id.ToString()[..8].ToUpperInvariant()}";

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
                $"Case referred to AGSA for enforcement. Reference {referralNumber}. {reason}",
                _currentUser.UserId,
                cancellationToken);

            try
            {
                await _emailService.SendAsync(new EmailMessageModel
                {
                    ToAddress = settings.AgsaReferralEmail,
                    CcAddress = settings.EnforcementUnitEmail,
                    Subject = $"CIDB Enforcement Referral - {referralNumber} - {tender.TenderNumber}",
                    Body =
                        $"<p>A compliance matter has been referred for enforcement.</p>" +
                        $"<p><strong>Reference:</strong> {referralNumber}<br/>" +
                        $"<strong>Tender:</strong> {tender.TenderNumber} - {tender.Title}<br/>" +
                        $"<strong>Employer:</strong> {tender.EmployerName}<br/>" +
                        $"<strong>Reason:</strong> {reason}</p>" +
                        "<p>Please find the full referral document attached.</p>",
                    AttachmentPaths = string.IsNullOrWhiteSpace(document.FilePath)
                        ? new List<string>()
                        : new List<string> { document.FilePath }
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                // Don't let a failed email undo the referral that already happened - it's
                // logged and the document is on record; this just needs manual follow-up.
                _logger.LogError(
                    ex,
                    "AGSA referral {ReferralNumber} was created but the notification email failed to send.",
                    referralNumber);

                await _auditService.LogAsync(
                    AuditAction.Error,
                    AuditEntity.ComplianceCase,
                    complianceCase.Id,
                    $"AGSA referral {referralNumber} created, but the email to AGSA/enforcement failed: {ex.Message}",
                    _currentUser.UserId,
                    cancellationToken);
            }

            var managerIds = await _userService.GetManagerUserIdsAsync(cancellationToken);

            foreach (var managerId in managerIds)
            {
                await _notificationService.NotifyAsync(new CreateNotificationModel
                {
                    UserId = managerId,
                    Title = "Case Referred for Enforcement (SLA Breach)",
                    Message =
                        $"Tender {tender.TenderNumber} has been referred to AGSA ({referralNumber}). " +
                        $"The {settings.AgsaReferralDeadlineDays}-day SLA target was exceeded. {reason}",
                    Type = NotificationType.Error,
                    Url = $"/cases/{complianceCase.Id}"
                }, cancellationToken);
            }

            return true;
        }

        /// <summary>
        /// BR-12: before issuing a subsequent notice, confirm the case is still
        /// actually non-compliant. Returns false if there are no open findings,
        /// or the case is already resolved/closed.
        /// </summary>
        private static bool IsStillNonCompliant(ComplianceCase complianceCase)
        {
            if (complianceCase.Status == CaseStatus.Closed)
                return false;

            if (complianceCase.Outcome == ComplianceOutcome.Compliant)
                return false;

            if (complianceCase.ComplianceFindings == null
                || !complianceCase.ComplianceFindings.Any(f => !f.IsResolved))
                return false;

            return true;
        }

        public async Task<int> RunSlaWarningsAsync(CancellationToken cancellationToken = default)
        {
            var settings = await _systemSettingService.GetAsync();

            var warningCutoff = DateTime.UtcNow.AddDays(-settings.SlaWarningDays);
            var breachCutoff = DateTime.UtcNow.AddDays(-settings.AgsaReferralDeadlineDays);

            // Cases past the warning cutoff.
            var cases = await _complianceCaseRepository
                .GetOpenCasesAssignedBeforeAsync(warningCutoff, cancellationToken);

            // Exclude cases that are already past the breach cutoff - those go
            // through the breach/AGSA path instead.
            var toWarn = cases
                .Where(c => c.AssignedOn.HasValue && c.AssignedOn.Value > breachCutoff)
                .ToList();

            var managerIds = await _userService
                .GetManagerUserIdsAsync(cancellationToken);

            var count = 0;

            foreach (var complianceCase in toWarn)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var daysOpen = complianceCase.AssignedOn.HasValue
                        ? (DateTime.UtcNow - complianceCase.AssignedOn.Value).Days
                        : 0;

                    var recipients = new HashSet<Guid>(managerIds);

                    if (complianceCase.AgentId.HasValue)
                    {
                        var coUserId = await _agentRepository
                            .GetUserIdByAgentIdAsync(
                                complianceCase.AgentId.Value, cancellationToken);

                        if (coUserId.HasValue)
                            recipients.Add(coUserId.Value);
                    }

                    foreach (var userId in recipients)
                    {
                        await _notificationService.NotifyAsync(new CreateNotificationModel
                        {
                            UserId = userId,
                            Title = "SLA Warning",
                            Message =
                                $"Case {complianceCase.Tender.TenderNumber} " +
                                $"({complianceCase.Tender.EmployerName}) has been open for " +
                                $"{daysOpen} days. The {settings.AgsaReferralDeadlineDays}-day " +
                                "target is approaching.",
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
                        "Failed to send SLA warning for case {CaseId}.",
                        complianceCase.Id);

                    await _auditService.LogAsync(
                        AuditAction.Error,
                        AuditEntity.ComplianceCase,
                        complianceCase.Id,
                        $"SLA warning notification failed: {ex.Message}",
                        _currentUser.UserId,
                        cancellationToken);
                }
            }

            _logger.LogInformation(
                "SLA warning pass complete - {Count} case(s) warned.", count);

            return count;
        }
    }
}