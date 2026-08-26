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
        private readonly IDocumentService _documentService;
        private readonly IEmailService _emailService;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CorrespondenceService> _logger;

        public CorrespondenceService(
            IComplianceCaseRepository complianceCaseRepository,
            ICaseLetterRepository caseLetterRepository,
            IDocumentService documentService,
            IEmailService emailService,
            ICurrentUserService currentUser,
            IAuditService auditService,
            IUnitOfWork unitOfWork,
            ILogger<CorrespondenceService> logger)
        {
            _complianceCaseRepository = complianceCaseRepository;
            _caseLetterRepository = caseLetterRepository;
            _documentService = documentService;
            _emailService = emailService;
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

            // Update the case
            complianceCase.Status = CaseStatus.WaitingForResponse;
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
                $"Instruction letter sent to {model.RecipientName}.",
                _currentUser.UserId,
                cancellationToken);

            await DeliverAsync(
                letter,
                model.RecipientName,
                model.RecipientEmail,
                "Instruction Letter",
                $"Please find attached an Instruction Letter regarding tender {model.TenderNumber}. " +
                $"A response is required by {model.ResponseDueOn:dd MMM yyyy}.",
                cancellationToken);
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

            complianceCase.Status = CaseStatus.ContraventionNoticeIssued;
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
                $"Contravention Notice sent to {model.RecipientName}. Reason: {model.Reason}.",
                _currentUser.UserId,
                cancellationToken);

            await DeliverAsync(
                letter,
                model.RecipientName,
                model.RecipientEmail,
                "Contravention Notice",
                $"Please find attached a formal Contravention Notice regarding tender {model.TenderNumber}. " +
                $"A response is required by {model.ResponseDueOn:dd MMM yyyy}. Failure to respond may result in " +
                "referral for enforcement action.",
                cancellationToken);

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
            //complianceCase.Clo0 = model.Outcome == ComplianceOutcome.Compliant
            //    ? CaseClosureReason.ClientComplied
            //    : complianceCase.;

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
