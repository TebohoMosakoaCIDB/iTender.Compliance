using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace iTender.Compliance.Infrastructure.Services
{
    /// <summary>
    /// Sends one reminder for whichever letter (Instruction or Contravention Notice) is currently
    /// outstanding on a case, once the CIDB finalized "day 7" working-day mark is reached - as
    /// long as that letter isn't already overdue (EscalationService owns that) and hasn't already
    /// had a reminder sent for it specifically.
    /// </summary>
    public class ReminderService : IReminderService
    {
        private readonly ICaseLetterRepository _caseLetterRepository;
        private readonly ICorrespondenceService _correspondenceService;
        private readonly ISystemSettingService _systemSettingService;
        private readonly IWorkingDayCalculator _workingDayCalculator;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<ReminderService> _logger;

        public ReminderService(
            ICaseLetterRepository caseLetterRepository,
            ICorrespondenceService correspondenceService,
            ISystemSettingService systemSettingService,
            IWorkingDayCalculator workingDayCalculator,
            IAuditService auditService,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            ILogger<ReminderService> logger)
        {
            _caseLetterRepository = caseLetterRepository;
            _correspondenceService = correspondenceService;
            _systemSettingService = systemSettingService;
            _workingDayCalculator = workingDayCalculator;
            _auditService = auditService;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<int> ProcessRemindersAsync(
            CancellationToken cancellationToken = default)
        {
            var settings = await _systemSettingService.GetAsync();

            if (!settings.EnableAutomaticReminders)
                return 0;

            var active = await _caseLetterRepository.GetActiveWithCaseAsync(cancellationToken);

            // Outstanding IL or CN letters, one per case (the latest), that:
            //  - are not yet overdue (that's escalation's job)
            //  - haven't already had a reminder sent specifically for them
            var now = DateTime.UtcNow;

            var eligible = active
                .Where(l => l.Type == LetterType.Instruction || l.Type == LetterType.ContraventionNotice)
                .GroupBy(l => l.ComplianceCaseId)
                .Select(g => g.OrderByDescending(x => x.LetterNumber).First())
                .Where(l => l.ResponseDueOn > now)
                .Where(l => !l.ComplianceCase.CaseLetters.Any(
                    other => other.Type == LetterType.Reminder && other.CreatedOn > l.CreatedOn))
                .ToList();

            // Working-day math needs to check the holiday calendar, so it can't run inside
            // the LINQ predicate above - filter the (already much smaller) eligible set here.
            var candidates = new List<CaseLetter>();

            foreach (var letter in eligible)
            {
                var reminderDueOn = await _workingDayCalculator.AddWorkingDaysAsync(
                    letter.SentOn,
                    settings.ReminderAfterWorkingDays,
                    cancellationToken);

                if (reminderDueOn <= now)
                    candidates.Add(letter);
            }

            var sentCount = 0;

            foreach (var letter in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var complianceCase = letter.ComplianceCase;

                    await _correspondenceService.SendReminderLetterAsync(new SendReminderLetterModel
                    {
                        ComplianceCaseId = complianceCase.Id,
                        CaseLetterId = letter.Id,
                        RecipientName = letter.RecipientName,
                        RecipientEmail = letter.RecipientEmail,
                        TenderNumber = complianceCase.Tender.TenderNumber,
                        TenderTitle = complianceCase.Tender.Title,
                        EmployerName = complianceCase.Tender.EmployerName,
                        ClosingDate = complianceCase.Tender.ClosingDate,
                        OriginalSentOn = letter.SentOn,
                        ResponseDueOn = letter.ResponseDueOn,
                        ReminderNumber = 1
                    }, cancellationToken);

                    sentCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to generate reminder letter for case {ComplianceCaseId}.",
                        letter.ComplianceCaseId);

                    await _auditService.LogAsync(
                        AuditAction.Error,
                        AuditEntity.ComplianceCase,
                        letter.ComplianceCaseId,
                        $"Failed to generate reminder letter. {ex.Message}",
                        _currentUser.UserId,
                        cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return sentCount;
        }
    }
}