using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace iTender.Compliance.Infrastructure.Services
{
    public class CorrespondenceService : ICorrespondenceService
    {
        private readonly IComplianceCaseRepository _complianceCaseRepository;
        private readonly ICaseLetterRepository _caseLetterRepository;
        private readonly IAgentRepository _agentRepository;
        private readonly ISystemSettingService _systemSettingService;
        private readonly IDocumentSigningService _documentSigningService;
        private readonly IDocumentService _documentService;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CorrespondenceService> _logger;

        public CorrespondenceService(
            IComplianceCaseRepository complianceCaseRepository,
            ICaseLetterRepository caseLetterRepository,
            IAgentRepository agentRepository,
            ISystemSettingService systemSettingService,
            IDocumentSigningService documentSigningService,
            IDocumentService documentService,
            IEmailService emailService,
            INotificationService notificationService,
            ICurrentUserService currentUser,
            IAuditService auditService,
            IUnitOfWork unitOfWork,
            ILogger<CorrespondenceService> logger)
        {
            _complianceCaseRepository = complianceCaseRepository;
            _caseLetterRepository = caseLetterRepository;
            _agentRepository = agentRepository;
            _systemSettingService = systemSettingService;
            _documentSigningService = documentSigningService;
            _documentService = documentService;
            _emailService = emailService;
            _notificationService = notificationService;
            _auditService = auditService;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task SendInstructionLetterAsync(SendInstructionLetterModel model, CancellationToken cancellationToken = default)
        {
            // Get the case
            var complianceCase = await _complianceCaseRepository.GetByIdAsync(
                model.ComplianceCaseId,
                cancellationToken);

            if (complianceCase == null)
                throw new InvalidOperationException("Compliance case not found.");

            // Determine the next letter number
            var latestLetter = await _caseLetterRepository.GetLatestAsync(
                model.ComplianceCaseId,
                cancellationToken);

            var nextLetterNumber = latestLetter == null
                ? 1
                : latestLetter.LetterNumber + 1;

            // Generate the document
            var document = await _documentService.GenerateInstructionLetterAsync(
                model,
                cancellationToken);

            // Create the correspondence record
            var letter = new CaseLetter
            {
                ComplianceCaseId = model.ComplianceCaseId,

                Type = LetterType.Instruction,

                LetterNumber = nextLetterNumber,

                RecipientName = model.RecipientName,
                RecipientEmail = model.RecipientEmail,

                FileName = document.FileName,
                FilePath = document.FilePath,

                SentOn = DateTime.UtcNow,
                ResponseDueOn = model.ResponseDueOn
            };

            await _caseLetterRepository.AddAsync(
                letter,
                cancellationToken);

            // Persist the letter before anything downstream (e.g. SigningRequestService)
            // needs to look it up by Id - a fresh DB query won't see an unsaved entity.
            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            var routedForApproval = await TryRouteForApprovalAsync(
                letter,
                cancellationToken);

            // Update the case
            complianceCase.Status = routedForApproval
                ? CaseStatus.PendingApproval
                : CaseStatus.WaitingForResponse;

            complianceCase.ModifiedOn = DateTime.UtcNow;

            await _complianceCaseRepository.UpdateAsync(
                complianceCase,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            // Audit
            await _auditService.LogAsync(
                AuditAction.InstructionLetterSent,
                AuditEntity.ComplianceCase,
                complianceCase.Id,
                routedForApproval
                    ? $"Instruction letter generated for {model.RecipientName} and sent for Manager approval."
                    : $"Instruction letter sent to {model.RecipientName}.",
                _currentUser.UserId,
                cancellationToken);

            if (!routedForApproval)
            {
                await DeliverAsync(
                    letter,
                    model.RecipientName,
                    model.RecipientEmail,
                    "Instruction Letter",
                    $"Please find attached an Instruction Letter regarding tender {model.TenderNumber}. " +
                    $"A response is required by {model.ResponseDueOn:dd MMM yyyy}.",
                    cancellationToken);
            }
        }

        public async Task SendReminderLetterAsync(SendReminderLetterModel model, CancellationToken cancellationToken = default)
        {
            // Get the compliance case
            var complianceCase = await _complianceCaseRepository.GetByIdAsync(
                model.ComplianceCaseId,
                cancellationToken);

            if (complianceCase == null)
                throw new InvalidOperationException("Compliance case not found.");

            // Get the latest correspondence
            var latestLetter = await _caseLetterRepository.GetLatestAsync(
                model.ComplianceCaseId,
                cancellationToken);

            if (latestLetter == null)
                throw new InvalidOperationException("No instruction letter has been sent.");

            // Determine next letter number
            var nextLetterNumber = latestLetter.LetterNumber + 1;

            // Generate the reminder document
            var document = await _documentService.GenerateReminderLetterAsync(
                model,
                cancellationToken);

            // Create reminder letter
            var reminderLetter = new CaseLetter
            {
                ComplianceCaseId = model.ComplianceCaseId,

                Type = LetterType.Reminder,

                LetterNumber = nextLetterNumber,

                RecipientName = model.RecipientName,
                RecipientEmail = model.RecipientEmail,

                FileName = document.FileName,
                FilePath = document.FilePath,

                SentOn = DateTime.UtcNow,

                // Usually keep the original due date
                ResponseDueOn = model.ResponseDueOn
            };

            await _caseLetterRepository.AddAsync(
                reminderLetter,
                cancellationToken);

            // Update the case
            complianceCase.ModifiedOn = DateTime.UtcNow;

            await _complianceCaseRepository.UpdateAsync(
                complianceCase,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            // Audit
            await _auditService.LogAsync(
                AuditAction.ReminderLetterSent,
                AuditEntity.ComplianceCase,
                complianceCase.Id,
                $"Reminder letter #{model.ReminderNumber} sent to {model.RecipientName}.",
                _currentUser.UserId,
                cancellationToken);

            await DeliverAsync(
                reminderLetter,
                model.RecipientName,
                model.RecipientEmail,
                "Reminder Letter",
                $"This is a reminder regarding our earlier Instruction Letter for tender {model.TenderNumber}. " +
                $"A response is still required by {model.ResponseDueOn:dd MMM yyyy}.",
                cancellationToken);
        }

        public async Task<Guid> SendContraventionNoticeAsync(SendContraventionNoticeModel model, CancellationToken cancellationToken = default)
        {
            var complianceCase = await _complianceCaseRepository.GetByIdAsync(
                model.ComplianceCaseId,
                cancellationToken);

            if (complianceCase == null)
                throw new InvalidOperationException("Compliance case not found.");

            var latestLetter = await _caseLetterRepository.GetLatestAsync(
                model.ComplianceCaseId,
                cancellationToken);

            var nextLetterNumber = latestLetter == null
                ? 1
                : latestLetter.LetterNumber + 1;

            var document = await _documentService.GenerateContraventionNoticeAsync(
                model,
                cancellationToken);

            var letter = new CaseLetter
            {
                ComplianceCaseId = model.ComplianceCaseId,

                Type = LetterType.ContraventionNotice,

                LetterNumber = nextLetterNumber,

                RecipientName = model.RecipientName,
                RecipientEmail = model.RecipientEmail,

                FileName = document.FileName,
                FilePath = document.FilePath,

                SentOn = DateTime.UtcNow,
                ResponseDueOn = model.ResponseDueOn
            };

            await _caseLetterRepository.AddAsync(
                letter,
                cancellationToken);

            // Persist the letter before anything downstream (e.g. SigningRequestService)
            // needs to look it up by Id - a fresh DB query won't see an unsaved entity.
            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            var routedForApproval = await TryRouteForApprovalAsync(
                letter,
                cancellationToken);

            complianceCase.Status = routedForApproval
                ? CaseStatus.PendingApproval
                : CaseStatus.ContraventionNoticeIssued;

            complianceCase.ModifiedOn = DateTime.UtcNow;

            await _complianceCaseRepository.UpdateAsync(
                complianceCase,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            await _auditService.LogAsync(
                AuditAction.ContraventionNoticeSent,
                AuditEntity.ComplianceCase,
                complianceCase.Id,
                routedForApproval
                    ? $"Contravention Notice generated for {model.RecipientName} and sent for Manager approval. Reason: {model.Reason}."
                    : $"Contravention Notice sent to {model.RecipientName}. Reason: {model.Reason}.",
                _currentUser.UserId,
                cancellationToken);

            if (!routedForApproval)
            {
                await DeliverAsync(
                    letter,
                    model.RecipientName,
                    model.RecipientEmail,
                    "Contravention Notice",
                    $"Please find attached a formal Contravention Notice regarding tender {model.TenderNumber}. " +
                    $"A response is required by {model.ResponseDueOn:dd MMM yyyy}. Failure to respond may result in " +
                    "referral for enforcement action.",
                    cancellationToken);
            }

            return letter.Id;
        }

        public async Task RecordResponseAsync(
            Guid caseLetterId,
            bool accepted,
            string? comments,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var letter = await _caseLetterRepository.GetByIdAsync(
                caseLetterId,
                cancellationToken);

            if (letter == null)
                throw new InvalidOperationException("Case letter not found.");

            letter.RespondedOn = DateTime.UtcNow;
            letter.Accepted = accepted;
            letter.ResponseComments = comments;
            letter.ModifiedBy = userId;
            letter.ModifiedOn = DateTime.UtcNow;

            await _caseLetterRepository.UpdateAsync(letter, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.ResponseReceived,
                AuditEntity.CaseLetter,
                letter.Id,
                accepted
                    ? "Response accepted."
                    : "Response rejected.",
                userId,
                cancellationToken);
        }

        public async Task CaptureResponseAsync(
    CaptureResponseModel model,
    CancellationToken cancellationToken = default)
        {
            var complianceCase = await _complianceCaseRepository.GetByIdAsync(
                model.ComplianceCaseId,
                cancellationToken);

            if (complianceCase == null)
                throw new InvalidOperationException("Compliance case not found.");

            var letter = await _caseLetterRepository.GetByIdAsync(
                model.CaseLetterId,
                cancellationToken);

            if (letter == null)
                throw new InvalidOperationException("Letter not found.");

            letter.RespondedOn = model.RespondedOn;
            letter.Accepted = model.Outcome == ComplianceOutcome.Compliant;
            letter.ResponseComments = model.Comments;

            await _caseLetterRepository.UpdateAsync(letter, cancellationToken);

            complianceCase.Status = CaseStatus.Closed;
            complianceCase.Outcome = model.Outcome;
            complianceCase.ClosedDate = model.RespondedOn;
            complianceCase.ModifiedOn = DateTime.UtcNow;
            complianceCase.Comments = model.Comments;
            complianceCase.ClosureReason = model.Outcome == ComplianceOutcome.Compliant
                ? CaseClosureReason.ClientComplied
                : null;

            await _complianceCaseRepository.UpdateAsync(
                complianceCase,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.ResponseReceived,
                AuditEntity.ComplianceCase,
                complianceCase.Id,
                $"Response received ({model.Outcome}).",
                _currentUser.UserId,
                cancellationToken);

            await _auditService.LogAsync(
                AuditAction.CaseClosed,
                AuditEntity.ComplianceCase,
                complianceCase.Id,
                "Compliance case closed.",
                _currentUser.UserId,
                cancellationToken);
        }

        /// <summary>Routes a generated letter to the active Manager for sign-off via SigningHub if
        /// RequireManagerApproval is on and a Manager exists. Falls back to immediate delivery (returns
        /// false) if approval isn't required, no Manager is configured, or SigningHub is unreachable -
        /// a compliance deadline should never be silently blocked by a third-party outage.</summary>
        private async Task<bool> TryRouteForApprovalAsync(
            CaseLetter letter,
            CancellationToken cancellationToken)
        {
            var settings = await _systemSettingService.GetAsync();

            if (!settings.RequireManagerApproval)
                return false;

            var managers = await _agentRepository.GetManagersAsync(cancellationToken);
            var manager = managers.FirstOrDefault();

            if (manager == null)
            {
                _logger.LogWarning(
                    "RequireManagerApproval is enabled but no active Manager is configured. " +
                    "Sending case letter {CaseLetterId} without approval.",
                    letter.Id);

                return false;
            }

            try
            {
                await _documentSigningService.RequestApprovalAsync(
                    letter,
                    manager,
                    cancellationToken);

                await _auditService.LogAsync(
                    AuditAction.ApprovalRequested,
                    AuditEntity.CaseLetter,
                    letter.Id,
                    $"Sent to {manager.FullName} for approval via SigningHub.",
                    _currentUser.UserId,
                    cancellationToken);

                if (manager.UserId != Guid.Empty)
                {
                    await _notificationService.NotifyAsync(new CreateNotificationModel
                    {
                        UserId = manager.UserId,
                        Title = "Letter Awaiting Your Approval",
                        Message = $"A {letter.Type} for {letter.RecipientName} is awaiting your sign-off in SigningHub.",
                        Type = NotificationType.Information
                    }, cancellationToken);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to route case letter {CaseLetterId} to SigningHub for approval; sending directly instead.",
                    letter.Id);

                await _auditService.LogAsync(
                    AuditAction.Error,
                    AuditEntity.CaseLetter,
                    letter.Id,
                    $"Approval routing failed, letter sent without sign-off: {ex.Message}",
                    _currentUser.UserId,
                    cancellationToken);

                return false;
            }
        }

        public async Task CompleteApprovedLetterAsync(
            Guid caseLetterId,
            CancellationToken cancellationToken = default)
        {
            var letter = await _caseLetterRepository.GetByIdAsync(
                caseLetterId,
                cancellationToken);

            if (letter == null)
                return;

            var complianceCase = await _complianceCaseRepository.GetByIdAsync(
                letter.ComplianceCaseId,
                cancellationToken);

            if (complianceCase == null)
                return;

            complianceCase.Status = letter.Type == LetterType.ContraventionNotice
                ? CaseStatus.ContraventionNoticeIssued
                : CaseStatus.WaitingForResponse;

            complianceCase.ModifiedOn = DateTime.UtcNow;

            await _complianceCaseRepository.UpdateAsync(
                complianceCase,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.ApprovalGranted,
                AuditEntity.CaseLetter,
                letter.Id,
                "Manager approved the letter via SigningHub.",
                cancellationToken: cancellationToken);

            await DeliverAsync(
                letter,
                letter.RecipientName,
                letter.RecipientEmail,
                letter.Type.ToString(),
                $"Please find attached the {letter.Type} regarding this compliance matter. " +
                $"A response is required by {letter.ResponseDueOn:dd MMM yyyy}.",
                cancellationToken);
        }

        public async Task HandleRejectedLetterAsync(
            Guid caseLetterId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            var letter = await _caseLetterRepository.GetByIdAsync(
                caseLetterId,
                cancellationToken);

            if (letter == null)
                return;

            var complianceCase = await _complianceCaseRepository.GetByIdAsync(
                letter.ComplianceCaseId,
                cancellationToken);

            if (complianceCase == null)
                return;

            // The letter was never sent - route the case back to the officer
            // rather than leaving it stuck awaiting an approval that failed.
            complianceCase.Status = complianceCase.AgentId.HasValue
                ? CaseStatus.Assigned
                : CaseStatus.New;

            complianceCase.ModifiedOn = DateTime.UtcNow;

            await _complianceCaseRepository.UpdateAsync(
                complianceCase,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.ApprovalRejected,
                AuditEntity.CaseLetter,
                letter.Id,
                $"Manager rejected the {letter.Type} via SigningHub: {reason}",
                cancellationToken: cancellationToken);

            if (complianceCase.AgentId.HasValue)
            {
                await _notificationService.NotifyAsync(new CreateNotificationModel
                {
                    UserId = complianceCase.AgentId,
                    Title = "Letter Rejected by Manager",
                    Message = $"The {letter.Type} for this case was rejected: {reason}",
                    Type = NotificationType.Warning,
                    Url = $"/cases/{complianceCase.Id}"
                }, cancellationToken);
            }
        }

        /// <summary>
        /// Emails the generated letter to the client and marks it as sent.
        /// Delivery failures are logged to the audit trail rather than thrown,
        /// so a transient SMTP issue never rolls back an already-generated,
        /// already-recorded letter - it can be resent manually.
        /// </summary>
        private async Task DeliverAsync(
            CaseLetter letter,
            string recipientName,
            string recipientEmail,
            string documentLabel,
            string bodyMessage,
            CancellationToken cancellationToken)
        {
            try
            {
                await _emailService.SendAsync(new EmailMessageModel
                {
                    ToAddress = recipientEmail,
                    ToName = recipientName,
                    Subject = $"{documentLabel} - Compliance Notice",
                    Body = $"<p>Dear {recipientName},</p><p>{bodyMessage}</p><p>Regards,<br/>iTender Compliance</p>",
                    AttachmentPaths = new List<string> { letter.FilePath }
                }, cancellationToken);

                letter.EmailSent = true;

                await _caseLetterRepository.UpdateAsync(letter, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to email {DocumentLabel} for case letter {CaseLetterId} to {Recipient}.",
                    documentLabel,
                    letter.Id,
                    recipientEmail);

                await _auditService.LogAsync(
                    AuditAction.Error,
                    AuditEntity.CaseLetter,
                    letter.Id,
                    $"Failed to email {documentLabel} to {recipientEmail}: {ex.Message}",
                    _currentUser.UserId,
                    cancellationToken);
            }
        }
    }
}