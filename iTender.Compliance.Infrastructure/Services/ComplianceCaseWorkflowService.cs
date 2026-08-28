using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace iTender.Compliance.Infrastructure.Services
{
    public class ComplianceCaseWorkflowService : IComplianceCaseWorkflowService
    {
        private readonly IComplianceCaseRepository _caseRepository;
        private readonly IComplianceFindingRepository _findingRepository;
        private readonly IComplianceActionRepository _actionRepository;
        private readonly ICaseLetterRepository _letterRepository;
        //private readonly IManagerApprovalRepository _approvalRepository;
        private readonly ISystemSettingRepository _settingRepository;
        private readonly IAuditService _auditService;
        private readonly ILetterNumberGenerator _letterNumberGenerator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public Task ApproveCorrespondenceAsync(Guid actionId, string? comments, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task IssueContraventionNoticeAsync(Guid caseId, Guid findingId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task IssueErratumAsync(Guid caseId, Guid findingId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        // ... constructor

        public async Task IssueInstructionalLetterAsync(Guid caseId, Guid findingId, CancellationToken ct)
        {
            var caseEntity = await _caseRepository.GetByIdAsync(caseId, ct);
            var finding = await _findingRepository.GetByIdAsync(findingId, ct);
            // Determine response hours from settings (48h for IL)
            var settings = await _settingRepository.GetAsync(ct);
            var dueDate = DateTime.UtcNow.AddHours(settings.ResponseDueHours);
            // Create action
            var action = new ComplianceAction
            {
                ComplianceCaseId = caseId,
                ActionType = ComplianceActionType.InstructionalLetterSent,
                Status = ComplianceActionStatus.Pending,
                ActionDate = DateTime.UtcNow,
                ResponseDueDate = dueDate,
                Comments = "IL prepared"
            };
            await _actionRepository.AddAsync(action);
            // Generate letter number
            int letterNumber = await _letterNumberGenerator.GetNextNumberAsync(LetterType.Instruction);
            // Create letter
            var letter = new CaseLetter
            {
                ComplianceCaseId = caseId,
                Type = LetterType.Instruction,
                LetterNumber = letterNumber,
                RecipientName = caseEntity.Tender.EmployerName,
                RecipientEmail = caseEntity.Tender.ContactEmail,
                SentOn = DateTime.UtcNow, // not sent yet
                ResponseDueOn = dueDate,
                ComplianceFindingId = findingId,
                //Status = LetterType.
            };
            await _letterRepository.AddAsync(letter);
            // Update case status
            caseEntity.Status = CaseStatus.WaitingForResponse;
            await _caseRepository.UpdateAsync(caseEntity);
            await _unitOfWork.SaveChangesAsync(ct);
            // Submit for manager approval (if not auto-approve)
            await SubmitForManagerApprovalAsync(action.Id, ct);
        }

        public Task RejectCorrespondenceAsync(Guid actionId, string reason, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task SubmitForManagerApprovalAsync(Guid actionId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
        // ... similar for CN and Erratum (different response hours and action types)
    }
}
