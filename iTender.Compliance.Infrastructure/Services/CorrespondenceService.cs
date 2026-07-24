using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;

namespace iTender.Compliance.Infrastructure.Services
{
    public class CorrespondenceService : ICorrespondenceService
    {
        private readonly IComplianceCaseRepository _complianceCaseRepository;
        private readonly ICaseLetterRepository _caseLetterRepository;
        private readonly IDocumentService _documentService;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;

        public CorrespondenceService(
            IComplianceCaseRepository complianceCaseRepository,
            ICaseLetterRepository caseLetterRepository,
            IDocumentService documentService,
            ICurrentUserService currentUser,
            IAuditService auditService,
            IUnitOfWork unitOfWork)
        {
            _complianceCaseRepository = complianceCaseRepository;
            _caseLetterRepository = caseLetterRepository;
            _documentService = documentService;
            _auditService = auditService;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
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
    }
}
