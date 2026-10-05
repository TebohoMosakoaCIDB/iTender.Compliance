using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace iTender.Compliance.Infrastructure.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IAutoAssignmentService _autoAssignmentService;
        private readonly ISystemSettingService _systemSettingService;
        private readonly IComplianceCaseRepository _caseRepository;
        private readonly IAgentRepository _agentRepository;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AssignmentService> _logger;

        public AssignmentService(
            IAutoAssignmentService autoAssignmentService,
            IComplianceCaseRepository caseRepository,
            ISystemSettingService systemSettingService,
            IAgentRepository agentRepository,
            INotificationService notificationService,
            IAuditService auditService,
            IUnitOfWork unitOfWork,
            ILogger<AssignmentService> logger)
        {
            _autoAssignmentService = autoAssignmentService;
            _systemSettingService = systemSettingService;
            _caseRepository = caseRepository;
            _agentRepository = agentRepository;
            _notificationService = notificationService;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task AssignAsync(
            ComplianceCase @case,
            CancellationToken cancellationToken = default)
        {
            if (@case.AgentId.HasValue)
                return; // idempotent

            var settings = await _settingsOrDefaultsAsync(cancellationToken);

            if (!settings.AutoAssignmentEnabled)
            {
                _logger.LogInformation(
                    "Auto-assignment disabled. Case {CaseId} left unassigned for manual allocation.",
                    @case.Id);

                return;
            }

            var agent = await _autoAssignmentService
                .SelectAgentAsync(@case, settings.DistributionMethod, cancellationToken);

            if (agent is null)
            {
                _logger.LogInformation(
                    "No eligible agent for case {CaseId}. Left unassigned for manual allocation.",
                    @case.Id);

                return;
            }

            @case.AgentId = agent.Id;
            @case.AssignedOn = DateTime.UtcNow;
            @case.Status = CaseStatus.Assigned;
            @case.ModifiedOn = DateTime.UtcNow;

            await _caseRepository.UpdateAsync(@case, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.Assigned,
                AuditEntity.ComplianceCase,
                @case.Id,
                $"Case automatically assigned to {agent.FullName}.",
                null,
                cancellationToken);

            await _notificationService.NotifyAsync(new CreateNotificationModel
            {
                UserId = agent.UserId,
                Title = "New Case Assigned",
                Message = $"Tender {@case.Tender.TenderNumber} has been assigned to you.",
                Type = NotificationType.Information,
                Url = $"/cases/{@case.Id}"
            });
        }

        public async Task ManuallyAssignAsync(
            Guid caseId,
            Guid agentId,
            Guid managerUserId,
            string? comment,
            CancellationToken cancellationToken = default)
        {
            var @case = await _caseRepository.GetByIdAsync(caseId, cancellationToken)
                ?? throw new InvalidOperationException("Compliance case not found.");

            if (@case.AgentId.HasValue)
                throw new InvalidOperationException("Case is already assigned.");

            var agent = await _agentRepository.GetByIdAsync(agentId, cancellationToken)
                ?? throw new InvalidOperationException("Agent not found.");

            if (!agent.IsActive)
                throw new InvalidOperationException("Agent is not active.");

            @case.AgentId = agent.Id;
            @case.AssignedOn = DateTime.UtcNow;
            @case.Status = CaseStatus.Assigned;
            @case.ModifiedBy = managerUserId;
            @case.ModifiedOn = DateTime.UtcNow;

            await _caseRepository.UpdateAsync(@case, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.Assigned,
                AuditEntity.ComplianceCase,
                @case.Id,
                string.IsNullOrWhiteSpace(comment)
                    ? $"Case manually assigned to {agent.FullName} by manager."
                    : $"Case manually assigned to {agent.FullName} by manager: {comment}",
                managerUserId,
                cancellationToken);

            await _notificationService.NotifyAsync(new CreateNotificationModel
            {
                UserId = agent.UserId,
                Title = "New Case Assigned",
                Message = $"Tender {@case.Tender.TenderNumber} has been assigned to you.",
                Type = NotificationType.Information,
                Url = $"/cases/{@case.Id}"
            });
        }

        private async Task<SystemSettingModel> _settingsOrDefaultsAsync(
            CancellationToken cancellationToken)
        {
            // Adjust to whatever ISystemSettingService / ISystemSettingRepository exposes.
            var settings = await _systemSettingService.GetAsync();
            return settings;
        }
    }
}
