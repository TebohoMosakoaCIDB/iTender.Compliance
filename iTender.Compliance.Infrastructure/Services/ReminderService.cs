using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace iTender.Compliance.Infrastructure.Services
{
    public class ReminderService : IReminderService
    {
        private readonly IComplianceCaseRepository _complianceCaseRepository;
        private readonly ICorrespondenceService _correspondenceService;
        private readonly ISystemSettingService _systemSettingService;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<ReminderService> _logger;

        public ReminderService(
            IComplianceCaseRepository complianceCaseRepository,
            ICorrespondenceService correspondenceService,
            ISystemSettingService systemSettingService,
            IAuditService auditService,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            ILogger<ReminderService> logger)
        {
            _complianceCaseRepository = complianceCaseRepository;
            _correspondenceService = correspondenceService;
            _systemSettingService = systemSettingService;
            _auditService = auditService;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ProcessRemindersAsync(
            CancellationToken cancellationToken = default)
        {
            var settings = await _systemSettingService.GetAsync();

            if (!settings.EnableAutomaticReminders)
                return;

            var cases = await _complianceCaseRepository
                .GetCasesAwaitingReminderAsync(
                    settings.ReminderAfterHours,
                    cancellationToken);

            foreach (var complianceCase in cases)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // The instruction letter still outstanding on this case -
                    // GetCasesAwaitingReminderAsync guarantees exactly this exists.
                    var instructionLetter = complianceCase.CaseLetters
                        .Where(l => l.Type == LetterType.Instruction)
                        .OrderByDescending(l => l.LetterNumber)
                        .FirstOrDefault();

                    if (instructionLetter == null)
                    {
                        _logger.LogWarning(
                            "Case {ComplianceCaseId} was selected for a reminder but has no Instruction letter.",
                            complianceCase.Id);

                        continue;
                    }

                    var recipientName = instructionLetter.RecipientName;
                    var recipientEmail = instructionLetter.RecipientEmail;

                    await _correspondenceService.SendReminderLetterAsync(new SendReminderLetterModel
                    {
                        ComplianceCaseId = complianceCase.Id,
                        CaseLetterId = instructionLetter.Id,
                        RecipientName = recipientName,
                        RecipientEmail = recipientEmail,
                        TenderNumber = complianceCase.Tender.TenderNumber,
                        TenderTitle = complianceCase.Tender.Title,
                        EmployerName = complianceCase.Tender.EmployerName,
                        ClosingDate = complianceCase.Tender.ClosingDate,
                        OriginalSentOn = instructionLetter.SentOn,
                        ResponseDueOn = instructionLetter.ResponseDueOn,
                        ReminderNumber = 1
                    }, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to generate reminder letter for case {ComplianceCaseId}.",
                        complianceCase.Id);

                    await _auditService.LogAsync(
                        AuditAction.Error,
                        AuditEntity.ComplianceCase,
                        complianceCase.Id,
                        $"Failed to generate reminder letter. {ex.Message}",
                        _currentUser.UserId,
                        cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}