using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace iTender.Compliance.Infrastructure.Services
{
    public class ComplianceFollowUpService : IComplianceFollowUpService
    {
        private readonly ICaseLetterRepository _letterRepository;
        private readonly IComplianceCaseRepository _caseRepository;
        private readonly IComplianceActionRepository _actionRepository;
        private readonly ISystemSettingRepository _settingRepository;
        private readonly IAuditService _auditService;
        private readonly ILetterNumberGenerator _letterNumberGenerator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ComplianceFollowUpService> _logger;

        public ComplianceFollowUpService(
            ICaseLetterRepository letterRepository,
            IComplianceCaseRepository caseRepository,
            IComplianceActionRepository actionRepository,
            ISystemSettingRepository settingRepository,
            IAuditService auditService,
            ILetterNumberGenerator letterNumberGenerator,
            IUnitOfWork unitOfWork,
            ILogger<ComplianceFollowUpService> logger)
        {
            _letterRepository = letterRepository;
            _caseRepository = caseRepository;
            _actionRepository = actionRepository;
            _settingRepository = settingRepository;
            _auditService = auditService;
            _letterNumberGenerator = letterNumberGenerator;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ProcessOverdueResponsesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Processing overdue responses...");

            var overdueLetters = await _letterRepository.GetOutstandingAsync(cancellationToken);
            if (!overdueLetters.Any())
            {
                _logger.LogInformation("No overdue letters found.");
                return;
            }

            // Group by case – each case may have multiple overdue letters, but we only act on the latest letter.
            var groups = overdueLetters.GroupBy(l => l.ComplianceCaseId);
            foreach (var group in groups)
            {
                var caseId = group.Key;
                var caseEntity = group.First().ComplianceCase; // includes Tender
                // Get the most recent letter for this case (by SentOn)
                var latestLetter = group.OrderByDescending(l => l.SentOn).First();

                _logger.LogInformation("Processing overdue case {CaseId}, latest letter type {LetterType} sent on {SentOn}",
                    caseId, latestLetter.Type, latestLetter.SentOn);

                // Determine action based on current case status (or letter type)
                if (caseEntity.Status == CaseStatus.WaitingForResponse)
                {
                    // No response to IL -> issue CN
                    await IssueContraventionNoticeAsync(caseEntity, cancellationToken);
                }
                else if (caseEntity.Status == CaseStatus.WaitingForResponse)
                {
                    // No response to CN -> escalate to AGSA
                    await EscalateToAGSAAsync(caseEntity.Id, cancellationToken);
                }
                else
                {
                    _logger.LogWarning("Case {CaseId} has overdue letters but status is {Status}, skipping.", caseId, caseEntity.Status);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Finished processing overdue responses.");
        }

        private async Task IssueContraventionNoticeAsync(ComplianceCase caseEntity, CancellationToken cancellationToken)
        {
            var settings = await _settingRepository.GetAsync(cancellationToken);
            if (settings == null)
                throw new InvalidOperationException("System settings not found.");

            // Create a new Contravention Notice letter
            var letterNumber = await _letterNumberGenerator.GetNextNumberAsync(LetterType.ContraventionNotice, cancellationToken);
            var dueDate = DateTime.UtcNow.AddDays(settings.ContraventionNoticeResponseDays);

            var letter = new CaseLetter
            {
                ComplianceCaseId = caseEntity.Id,
                Type = LetterType.ContraventionNotice,
                LetterNumber = letterNumber,
                RecipientName = caseEntity.Tender?.EmployerName ?? string.Empty,
                RecipientEmail = caseEntity.Tender?.ContactEmail ?? string.Empty,
                SentOn = DateTime.UtcNow,
                ResponseDueOn = dueDate,
                EmailSent = false,
                FileName = string.Empty,
                FilePath = string.Empty,
                // Optionally link to the first finding
                ComplianceFindingId = caseEntity.ComplianceFindings?.FirstOrDefault()?.Id
            };
            await _letterRepository.AddAsync(letter, cancellationToken);

            // Create action
            var action = new ComplianceAction
            {
                ComplianceCaseId = caseEntity.Id,
                ActionType = ComplianceActionType.ContraventionNoticeSent,
                Status = ComplianceActionStatus.Pending,
                ActionDate = DateTime.UtcNow,
                ResponseDueDate = dueDate,
                Comments = "Contravention Notice issued after no response to Instructional Letter."
            };
            await _actionRepository.AddAsync(action, cancellationToken);

            // Update case status
            caseEntity.Status = CaseStatus.WaitingForResponse;
            await _caseRepository.UpdateAsync(caseEntity, cancellationToken);

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Created,
                AuditEntity.CaseLetter,
                letter.Id,
                $"Contravention Notice #{letterNumber} issued for tender {caseEntity.Tender?.TenderNumber}. Response due: {dueDate}",
                null, // system action, no user
                cancellationToken);

            _logger.LogInformation("Contravention Notice #{LetterNumber} issued for case {CaseId}", letterNumber, caseEntity.Id);
        }

        public async Task EscalateToAGSAAsync(Guid caseId, CancellationToken cancellationToken)
        {
            var caseEntity = await _caseRepository.GetByIdAsync(caseId, cancellationToken);
            if (caseEntity == null)
                throw new ArgumentException("Case not found.");

            // Create action
            var action = new ComplianceAction
            {
                ComplianceCaseId = caseEntity.Id,
                ActionType = ComplianceActionType.EscalatedToAGSA,
                Status = ComplianceActionStatus.Completed,
                ActionDate = DateTime.UtcNow,
                Comments = "Case escalated to AGSA for enforcement due to no response to Contravention Notice."
            };
            await _actionRepository.AddAsync(action, cancellationToken);

            // Update case
            caseEntity.Status = CaseStatus.ReferredForEnforcement;
            caseEntity.Outcome = ComplianceOutcome.Escalated;
            await _caseRepository.UpdateAsync(caseEntity, cancellationToken);

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Updated,
                AuditEntity.ComplianceCase,
                caseEntity.Id,
                "Case escalated to AGSA for enforcement.",
                null,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Case {CaseId} escalated to AGSA.", caseId);
        }

        public async Task ProcessExtensionRequestAsync(Guid caseId, int extensionHours, CancellationToken cancellationToken)
        {
            var caseEntity = await _caseRepository.GetByIdAsync(caseId, cancellationToken);
            if (caseEntity == null)
                throw new ArgumentException("Case not found.");

            // Find the latest unresponded letter for this case
            var latestLetter = caseEntity.CaseLetters
                .Where(l => l.RespondedOn == null)
                .OrderByDescending(l => l.SentOn)
                .FirstOrDefault();

            if (latestLetter == null)
                throw new InvalidOperationException("No pending letter found for extension.");

            // Extend the due date
            latestLetter.ResponseDueOn = latestLetter.ResponseDueOn.AddHours(extensionHours);
            await _letterRepository.UpdateAsync(latestLetter, cancellationToken);

            // Create action for extension
            var action = new ComplianceAction
            {
                ComplianceCaseId = caseEntity.Id,
                ActionType = ComplianceActionType.ExtensionGranted,
                Status = ComplianceActionStatus.Completed,
                ActionDate = DateTime.UtcNow,
                Comments = $"Extension of {extensionHours} hours granted. New due date: {latestLetter.ResponseDueOn}"
            };
            await _actionRepository.AddAsync(action, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.Updated,
                AuditEntity.CaseLetter,
                latestLetter.Id,
                $"Extension granted. New response due: {latestLetter.ResponseDueOn}",
                null,
                cancellationToken);

            _logger.LogInformation("Extension granted for case {CaseId}, new due date: {DueDate}", caseId, latestLetter.ResponseDueOn);
        }

        public async Task RecordResponseAsync(Guid letterId, bool accepted, string? comments, CancellationToken cancellationToken)
        {
            var letter = await _letterRepository.GetByIdAsync(letterId, cancellationToken);
            if (letter == null)
                throw new ArgumentException("Letter not found.");

            // Mark response
            letter.RespondedOn = DateTime.UtcNow;
            letter.Accepted = accepted;
            letter.ResponseComments = comments;
            await _letterRepository.UpdateAsync(letter, cancellationToken);

            var caseEntity = letter.ComplianceCase;
            if (caseEntity == null)
                throw new InvalidOperationException("Case not associated with letter.");

            // Update case based on response
            if (accepted)
            {
                // Acceptance – resolve the case
                caseEntity.Status = CaseStatus.Closed;
                caseEntity.ClosedDate = DateTime.UtcNow;
                caseEntity.Outcome = ComplianceOutcome.Compliant;
                // Optionally, mark all findings as resolved
                foreach (var finding in caseEntity.ComplianceFindings ?? Enumerable.Empty<ComplianceFinding>())
                {
                    finding.IsResolved = true;
                    finding.ResolvedOn = DateTime.UtcNow;
                }
            }
            else
            {
                // Objection – escalate to manager (UnderReview)
                caseEntity.Status = CaseStatus.UnderManagerReview;
                caseEntity.Outcome = ComplianceOutcome.UnderReview;
                var objectionAction = new ComplianceAction
                {
                    ComplianceCaseId = caseEntity.Id,
                    ActionType = ComplianceActionType.ObjectionRaised,
                    Status = ComplianceActionStatus.Pending,
                    ActionDate = DateTime.UtcNow,
                    Comments = $"Client objected to letter. Comments: {comments ?? "No comments provided."}"
                };
                await _actionRepository.AddAsync(objectionAction, cancellationToken);
            }

            await _caseRepository.UpdateAsync(caseEntity, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.Updated,
                AuditEntity.CaseLetter,
                letter.Id,
                $"Response recorded: Accepted = {accepted}. Comments: {comments}",
                null,
                cancellationToken);

            _logger.LogInformation("Response recorded for letter {LetterId}, case {CaseId}. Accepted: {Accepted}", letterId, caseEntity.Id, accepted);
        }
    }
}