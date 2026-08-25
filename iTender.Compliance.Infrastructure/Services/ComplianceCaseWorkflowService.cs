using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace iTender.Compliance.Infrastructure.Services
{
    public class ComplianceCaseWorkflowService
    : IComplianceCaseWorkflowService
    {
        private readonly IComplianceCaseRepository _caseRepository;
        private readonly IComplianceActionRepository _actionRepository;
        private readonly ICaseLetterRepository _caseLetterRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ComplianceCaseWorkflowService(
            IComplianceCaseRepository caseRepository,
            IComplianceActionRepository actionRepository,
            ICaseLetterRepository caseLetterRepository,
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork)
        {
            _caseRepository = caseRepository;
            _actionRepository = actionRepository;
            _caseLetterRepository = caseLetterRepository;
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
        }

        // =========================================================
        // ASSIGN CASE
        // =========================================================

        public async Task AssignCaseAsync(
            Guid caseId,
            Guid agentId,
            CancellationToken cancellationToken = default)
        {
            var complianceCase =
                await GetCaseAsync(
                    caseId,
                    cancellationToken);

            if (complianceCase.Status != CaseStatus.New)
            {
                throw new InvalidOperationException(
                    $"Case cannot be assigned while it is in " +
                    $"'{complianceCase.Status}' status.");
            }

            complianceCase.AgentId = agentId;
            complianceCase.AssignedOn = DateTime.UtcNow;
            complianceCase.Status = CaseStatus.Assigned;

            await _caseRepository.UpdateAsync(
                complianceCase,
                cancellationToken);

            await AddActionAsync(
                complianceCase,
                ComplianceActionType.CaseAssigned,
                ComplianceActionStatus.Completed,
                "Compliance case assigned to a Compliance Officer.",
                cancellationToken);

            await AddAuditAsync(
                complianceCase,
                AuditAction.CaseAssigned,
                $"Case assigned to agent {agentId}.",
                cancellationToken);
        }

        // =========================================================
        // STREAM 1/2 - OPEN TENDER - ERRATUM
        // =========================================================

        public async Task IssueErratumAsync(
            Guid caseId,
            CancellationToken cancellationToken = default)
        {
            var complianceCase =
                await GetCaseAsync(
                    caseId,
                    cancellationToken);

            if (complianceCase.Status != CaseStatus.Assigned &&
                complianceCase.Status != CaseStatus.UnderReview)
            {
                throw new InvalidOperationException(
                    "An Erratum can only be issued to an assigned " +
                    "or reviewed case.");
            }

            if (complianceCase.Tender == null)
            {
                throw new InvalidOperationException(
                    "The tender associated with the case could not be found.");
            }

            if (!IsTenderOpen(complianceCase.Tender))
            {
                throw new InvalidOperationException(
                    "An Erratum cannot be issued because the tender is closed. " +
                    "A Contravention Notice must be considered.");
            }

            var dueDate =
                AddWorkingDays(
                    DateTime.UtcNow,
                    2);

            complianceCase.Status =
                CaseStatus.AwaitingILResponse;

            await _caseRepository.UpdateAsync(
                complianceCase,
                cancellationToken);

            await CreateLetterRecordAsync(
                complianceCase,
                LetterType.Erratum,
                cancellationToken);

            await AddActionAsync(
                complianceCase,
                ComplianceActionType.InstructionalLetterSent,
                ComplianceActionStatus.Pending,
                "Erratum issued to employer to correct the non-compliant tender advertisement.",
                cancellationToken,
                dueDate);

            await AddAuditAsync(
                complianceCase,
                AuditAction.ErratumNoticeSent,
                $"Employer allowed 2 working days to correct the tender advertisement. " +
                $"Response due: {dueDate:dd MMM yyyy HH:mm}.",
                cancellationToken);
        }

        // =========================================================
        // STREAM 3 - INSTRUCTIONAL LETTER
        // =========================================================

        public async Task IssueInstructionAsync(
            Guid caseId,
            CancellationToken cancellationToken = default)
        {
            var complianceCase =
                await GetCaseAsync(
                    caseId,
                    cancellationToken);

            if (complianceCase.Status != CaseStatus.Assigned &&
                complianceCase.Status != CaseStatus.UnderReview)
            {
                throw new InvalidOperationException(
                    "An Instructional Letter can only be issued " +
                    "to an assigned or reviewed case.");
            }

            var dueDate =
                AddWorkingDays(
                    DateTime.UtcNow,
                    2);

            complianceCase.Status =
                CaseStatus.AwaitingILResponse;

            await _caseRepository.UpdateAsync(
                complianceCase,
                cancellationToken);

            await CreateLetterRecordAsync(
                complianceCase,
                LetterType.Instruction,
                cancellationToken);

            await AddActionAsync(
                complianceCase,
                ComplianceActionType.InstructionalLetterSent,
                ComplianceActionStatus.Pending,
                "Instructional Letter issued for project registration non-compliance.",
                cancellationToken,
                dueDate);

            await AddAuditAsync(
                complianceCase,
                AuditAction.InstructionalLetterSent,
                $"Client allowed 2 working days to respond. " +
                $"Response due: {dueDate:dd MMM yyyy HH:mm}.",
                cancellationToken);
        }

        // =========================================================
        // CONTRAVENTION NOTICE
        // =========================================================

        public async Task IssueContraventionNoticeAsync(
            Guid caseId,
            CancellationToken cancellationToken = default)
        {
            var complianceCase =
                await GetCaseAsync(
                    caseId,
                    cancellationToken);

            if (complianceCase.Status != CaseStatus.Assigned &&
                complianceCase.Status != CaseStatus.AwaitingILResponse &&
                complianceCase.Status != CaseStatus.UnderReview)
            {
                throw new InvalidOperationException(
                    $"A Contravention Notice cannot be issued while the " +
                    $"case is in '{complianceCase.Status}' status.");
            }

            var dueDate =
                AddWorkingDays(
                    DateTime.UtcNow,
                    14);

            complianceCase.Status =
                CaseStatus.AwaitingCNResponse;

            complianceCase.Outcome =
                ComplianceOutcome.NonCompliant;

            await _caseRepository.UpdateAsync(
                complianceCase,
                cancellationToken);

            await CreateLetterRecordAsync(
                complianceCase,
                LetterType.ContraventionNotice,
                cancellationToken);

            await AddActionAsync(
                complianceCase,
                ComplianceActionType.ContraventionNoticeSent,
                ComplianceActionStatus.Pending,
                "Contravention Notice issued to client.",
                cancellationToken,
                dueDate);

            await AddAuditAsync(
                complianceCase,
                AuditAction.ContraventionNoticeSent,
                $"Client allowed 14 working days to respond. " +
                $"Response due: {dueDate:dd MMM yyyy HH:mm}.",
                cancellationToken);
        }

        // =========================================================
        // REMINDER
        // =========================================================

        public async Task IssueReminderAsync(
            Guid caseId,
            CancellationToken cancellationToken = default)
        {
            var complianceCase =
                await GetCaseAsync(
                    caseId,
                    cancellationToken);

            if (complianceCase.Status != CaseStatus.AwaitingILResponse &&
                complianceCase.Status != CaseStatus.AwaitingCNResponse)
            {
                throw new InvalidOperationException(
                    "A reminder can only be issued while awaiting a client response.");
            }

            await CreateLetterRecordAsync(
                complianceCase,
                LetterType.Reminder,
                cancellationToken);

            await AddAuditAsync(
                complianceCase,
                AuditAction.ReminderLetterSent,
                "Reminder issued to client regarding outstanding response.",
                cancellationToken);
        }

        // =========================================================
        // RESPONSE
        // =========================================================

        public async Task RecordResponseAsync(
            Guid caseId,
            string? comments = null,
            CancellationToken cancellationToken = default)
        {
            var complianceCase =
                await GetCaseAsync(
                    caseId,
                    cancellationToken);

            if (complianceCase.Status != CaseStatus.AwaitingILResponse &&
                complianceCase.Status != CaseStatus.AwaitingCNResponse)
            {
                throw new InvalidOperationException(
                    "A client response cannot be recorded for this case " +
                    "in its current status.");
            }

            complianceCase.Status =
                CaseStatus.UnderReview;

            complianceCase.Outcome =
                ComplianceOutcome.UnderReview;

            if (!string.IsNullOrWhiteSpace(comments))
            {
                complianceCase.Comments =
                    comments;
            }

            await _caseRepository.UpdateAsync(
                complianceCase,
                cancellationToken);

            await AddActionAsync(
                complianceCase,
                ComplianceActionType.ResponseReceived,
                ComplianceActionStatus.Completed,
                comments ?? "Client response received.",
                cancellationToken);

            await AddAuditAsync(
                complianceCase,
                AuditAction.ResponseReceived,
                "Client response received. Case referred for review.",
                cancellationToken);
        }

        // =========================================================
        // EXTENSION REQUEST
        // =========================================================

        public async Task RequestExtensionAsync(
            Guid caseId,
            string? comments = null,
            CancellationToken cancellationToken = default)
        {
            var complianceCase =
                await GetCaseAsync(
                    caseId,
                    cancellationToken);

            if (complianceCase.Status != CaseStatus.AwaitingILResponse &&
                complianceCase.Status != CaseStatus.AwaitingCNResponse)
            {
                throw new InvalidOperationException(
                    "An extension can only be requested while awaiting a response.");
            }

            await AddActionAsync(
                complianceCase,
                ComplianceActionType.ExtensionRequested,
                ComplianceActionStatus.Pending,
                comments ?? "Client requested an extension.",
                cancellationToken);

            await AddAuditAsync(
                complianceCase,
                AuditAction.ExtensionRequested,
                comments ?? "Client requested an extension.",
                cancellationToken);
        }

        // =========================================================
        // GRANT EXTENSION
        // =========================================================

        public async Task GrantExtensionAsync(
            Guid caseId,
            DateTime newDueDate,
            string? comments = null,
            CancellationToken cancellationToken = default)
        {
            var complianceCase =
                await GetCaseAsync(
                    caseId,
                    cancellationToken);

            if (complianceCase.Status != CaseStatus.AwaitingILResponse &&
                complianceCase.Status != CaseStatus.AwaitingCNResponse)
            {
                throw new InvalidOperationException(
                    "An extension can only be granted while awaiting a response.");
            }

            if (newDueDate <= DateTime.UtcNow)
            {
                throw new InvalidOperationException(
                    "The new response due date must be in the future.");
            }

            await AddActionAsync(
                complianceCase,
                ComplianceActionType.ExtensionGranted,
                ComplianceActionStatus.Completed,
                comments ?? "Extension granted by Compliance Officer.",
                cancellationToken,
                newDueDate);

            await AddAuditAsync(
                complianceCase,
                AuditAction.ExtensionGranted,
                $"Response deadline extended to " +
                $"{newDueDate:dd MMM yyyy HH:mm}.",
                cancellationToken);
        }

        // =========================================================
        // OBJECTION
        // =========================================================

        public async Task RaiseObjectionAsync(
            Guid caseId,
            string? comments = null,
            CancellationToken cancellationToken = default)
        {
            var complianceCase =
                await GetCaseAsync(
                    caseId,
                    cancellationToken);

            if (complianceCase.Status != CaseStatus.AwaitingILResponse &&
                complianceCase.Status != CaseStatus.AwaitingCNResponse)
            {
                throw new InvalidOperationException(
                    "An objection can only be raised against an active " +
                    "Instructional Letter or Contravention Notice.");
            }

            await AddActionAsync(
                complianceCase,
                ComplianceActionType.ObjectionRaised,
                ComplianceActionStatus.Pending,
                comments ?? "Client objection raised and referred to Manager.",
                cancellationToken);

            await AddAuditAsync(
                complianceCase,
                AuditAction.ObjectionRaised,
                "Client objection referred to the Regulatory Compliance Manager.",
                cancellationToken);
        }

        // =========================================================
        // AGSA ESCALATION
        // =========================================================

        public async Task EscalateToAGSAAsync(
            Guid caseId,
            string? comments = null,
            CancellationToken cancellationToken = default)
        {
            var complianceCase =
                await GetCaseAsync(
                    caseId,
                    cancellationToken);

            if (complianceCase.Status != CaseStatus.AwaitingCNResponse &&
                complianceCase.Status != CaseStatus.UnderReview)
            {
                throw new InvalidOperationException(
                    "The case cannot be escalated to AGSA from its current status.");
            }

            complianceCase.Status =
                CaseStatus.Escalated;

            complianceCase.Outcome =
                ComplianceOutcome.Escalated;

            if (!string.IsNullOrWhiteSpace(comments))
            {
                complianceCase.Comments =
                    comments;
            }

            await _caseRepository.UpdateAsync(
                complianceCase,
                cancellationToken);

            await AddActionAsync(
                complianceCase,
                ComplianceActionType.EscalatedToAGSA,
                ComplianceActionStatus.Completed,
                comments ?? "Case referred to AGSA for enforcement.",
                cancellationToken);

            await AddAuditAsync(
                complianceCase,
                AuditAction.EscalatedToAGSA,
                "Client failed to comply. Matter referred to AGSA for enforcement.",
                cancellationToken);
        }

        // =========================================================
        // CLOSE
        // =========================================================

        public async Task CloseCaseAsync(
            Guid caseId,
            ComplianceOutcome outcome,
            string? comments = null,
            CancellationToken cancellationToken = default)
        {
            var complianceCase =
                await GetCaseAsync(
                    caseId,
                    cancellationToken);

            if (complianceCase.Status == CaseStatus.Closed)
            {
                throw new InvalidOperationException(
                    "The case is already closed.");
            }

            complianceCase.Status =
                CaseStatus.Closed;

            complianceCase.Outcome =
                outcome;

            complianceCase.ClosedDate =
                DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(comments))
            {
                complianceCase.Comments =
                    comments;
            }

            await _caseRepository.UpdateAsync(
                complianceCase,
                cancellationToken);

            await AddActionAsync(
                complianceCase,
                ComplianceActionType.CaseClosed,
                ComplianceActionStatus.Completed,
                comments ?? "Compliance case closed.",
                cancellationToken);

            await AddAuditAsync(
                complianceCase,
                AuditAction.CaseClosed,
                $"Case closed with outcome: {outcome}.",
                cancellationToken);
        }

        // =========================================================
        // HELPERS
        // =========================================================

        private async Task<ComplianceCase> GetCaseAsync(
            Guid caseId,
            CancellationToken cancellationToken)
        {
            var complianceCase =
                await _caseRepository.GetByIdAsync(
                    caseId);

            if (complianceCase == null)
            {
                throw new InvalidOperationException(
                    $"Compliance case '{caseId}' could not be found.");
            }

            return complianceCase;
        }

        private async Task AddActionAsync(
            ComplianceCase complianceCase,
            ComplianceActionType actionType,
            ComplianceActionStatus status,
            string comments,
            CancellationToken cancellationToken,
            DateTime? responseDueDate = null)
        {
            var action = new ComplianceAction
            {
                Id = Guid.NewGuid(),

                ComplianceCaseId =
                    complianceCase.Id,

                ActionType =
                    actionType,

                Status =
                    status,

                ActionDate =
                    DateTime.UtcNow,

                ResponseDueDate =
                    responseDueDate,

                Comments =
                    comments
            };

            await _actionRepository.AddAsync(
                action,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task CreateLetterRecordAsync(
            ComplianceCase complianceCase,
            LetterType letterType,
            CancellationToken cancellationToken)
        {
            var letter = new CaseLetter
            {
                Id = Guid.NewGuid(),

                ComplianceCaseId =
                    complianceCase.Id,

                Type =
                    letterType,

                CreatedOn =
                    DateTime.UtcNow
            };

            await _caseLetterRepository.AddAsync(
                letter,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task AddAuditAsync(
            ComplianceCase complianceCase,
            AuditAction action,
            string description,
            CancellationToken cancellationToken)
        {
            var audit = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityId = complianceCase.Id,
                Action = action,
                Description = description,
                CreatedOn = DateTime.UtcNow
            };

            await _auditLogRepository.AddAsync(audit, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static bool IsTenderOpen(Tender tender)
        {
            return tender.ClosingDate > DateTime.UtcNow;
        }

        private static DateTime AddWorkingDays(
            DateTime startDate,
            int numberOfWorkingDays)
        {
            var date = startDate;

            var daysAdded = 0;

            while (daysAdded < numberOfWorkingDays)
            {
                date = date.AddDays(1);

                if (date.DayOfWeek != DayOfWeek.Saturday &&
                    date.DayOfWeek != DayOfWeek.Sunday)
                {
                    daysAdded++;
                }
            }

            return date;
        }
    }
}
