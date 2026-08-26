using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace iTender.Compliance.Infrastructure.Services
{
    public class ObjectionService : IObjectionService
    {
        private readonly ICaseObjectionRepository _objectionRepository;
        private readonly IComplianceCaseRepository _complianceCaseRepository;
        private readonly ICaseLetterRepository _caseLetterRepository;
        private readonly IAgentRepository _agentRepository;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ObjectionService> _logger;

        public ObjectionService(
            ICaseObjectionRepository objectionRepository,
            IComplianceCaseRepository complianceCaseRepository,
            ICaseLetterRepository caseLetterRepository,
            IAgentRepository agentRepository,
            INotificationService notificationService,
            IAuditService auditService,
            ICurrentUserService currentUser,
            IUnitOfWork unitOfWork,
            ILogger<ObjectionService> logger)
        {
            _objectionRepository = objectionRepository;
            _complianceCaseRepository = complianceCaseRepository;
            _caseLetterRepository = caseLetterRepository;
            _agentRepository = agentRepository;
            _notificationService = notificationService;
            _auditService = auditService;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> RecordObjectionAsync(
            RecordObjectionModel model,
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

            if (letter == null || letter.ComplianceCaseId != model.ComplianceCaseId)
                throw new InvalidOperationException(
                    "The letter being objected to could not be found on this case.");

            if (complianceCase.Status == CaseStatus.Closed)
                throw new InvalidOperationException(
                    "Cannot record an objection against a closed case.");

            var objection = new CaseObjection
            {
                ComplianceCaseId = model.ComplianceCaseId,
                CaseLetterId = model.CaseLetterId,
                ReceivedOn = model.ReceivedOn,
                Reason = model.Reason,
                Status = ObjectionStatus.Received
            };

            await _objectionRepository.AddAsync(objection, cancellationToken);

            // Route the case to Manager review; the SLA clock effectively
            // pauses on the outstanding letter until the objection is resolved.
            complianceCase.Status = CaseStatus.UnderManagerReview;
            complianceCase.ModifiedOn = DateTime.UtcNow;

            await _complianceCaseRepository.UpdateAsync(complianceCase, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.ObjectionReceived,
                AuditEntity.ComplianceCase,
                complianceCase.Id,
                $"Client objection received against {letter.Type} #{letter.LetterNumber}: {model.Reason}",
                _currentUser.UserId,
                cancellationToken);

            var managers = await _agentRepository.GetManagersAsync(cancellationToken);

            foreach (var manager in managers)
            {
                await _notificationService.NotifyAsync(new CreateNotificationModel
                {
                    UserId = manager.Id,
                    Title = "Objection Requires Review",
                    Message = $"A client objection to the {letter.Type} on tender case has been received and needs your decision.",
                    Type = NotificationType.Warning,
                    Url = $"/cases/{complianceCase.Id}"
                }, cancellationToken);
            }

            if (managers.Count == 0)
            {
                _logger.LogWarning(
                    "Objection {ObjectionId} recorded for case {ComplianceCaseId} but no active Manager is configured to review it.",
                    objection.Id,
                    complianceCase.Id);
            }

            return objection.Id;
        }

        public async Task ResolveObjectionAsync(
            ResolveObjectionModel model,
            Guid resolvedByAgentId,
            CancellationToken cancellationToken = default)
        {
            var objection = await _objectionRepository.GetByIdAsync(
                model.ObjectionId,
                cancellationToken);

            if (objection == null)
                throw new InvalidOperationException("Objection not found.");

            if (objection.Status == ObjectionStatus.Resolved)
                throw new InvalidOperationException("This objection has already been resolved.");

            var complianceCase = objection.ComplianceCase;

            objection.Decision = model.Decision;
            objection.ManagerNotes = model.ManagerNotes;
            objection.ReviewedByAgentId = resolvedByAgentId;
            objection.ReviewedOn = DateTime.UtcNow;
            objection.Status = ObjectionStatus.Resolved;

            await _objectionRepository.UpdateAsync(objection, cancellationToken);

            if (model.Decision == ObjectionDecision.Upheld)
            {
                // Objection accepted - the client's position stands, close the case.
                complianceCase.Status = CaseStatus.Closed;
                complianceCase.ClosedDate = DateTime.UtcNow;
                complianceCase.ClosureReason = CaseClosureReason.ManagerOverride;
                complianceCase.Comments = model.ManagerNotes;
            }
            else
            {
                // Overruled - the case resumes on the track it was on when
                // the objection was raised, so escalation can continue.
                complianceCase.Status = objection.CaseLetter.Type == LetterType.ContraventionNotice
                    ? CaseStatus.ContraventionNoticeIssued
                    : CaseStatus.WaitingForResponse;
            }

            complianceCase.ModifiedOn = DateTime.UtcNow;

            await _complianceCaseRepository.UpdateAsync(complianceCase, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.ObjectionResolved,
                AuditEntity.ComplianceCase,
                complianceCase.Id,
                $"Objection {model.Decision}. {model.ManagerNotes}",
                _currentUser.UserId,
                cancellationToken);

            if (complianceCase.AgentId.HasValue)
            {
                await _notificationService.NotifyAsync(new CreateNotificationModel
                {
                    UserId = complianceCase.AgentId,
                    Title = "Objection Decision Made",
                    Message = model.Decision == ObjectionDecision.Upheld
                        ? "The Manager upheld the client's objection. The case has been closed."
                        : "The Manager overruled the client's objection. The case will continue through the workflow.",
                    Type = model.Decision == ObjectionDecision.Upheld
                        ? NotificationType.Information
                        : NotificationType.Warning,
                    Url = $"/cases/{complianceCase.Id}"
                }, cancellationToken);
            }
        }
    }
}