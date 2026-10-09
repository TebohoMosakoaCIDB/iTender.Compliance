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
        private readonly IUserService _userService;
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
            IUserService userService,
            INotificationService notificationService,
            IAuditService auditService,
            IUnitOfWork unitOfWork,
            ILogger<AssignmentService> logger)
        {
            _autoAssignmentService = autoAssignmentService;
            _systemSettingService = systemSettingService;
            _caseRepository = caseRepository;
            _agentRepository = agentRepository;
            _userService = userService;
            _notificationService = notificationService;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Statuses from which a case may be promoted to Assigned.
        /// Cases already in Cleared / Closed / Referral etc. keep their status.
        /// </summary>
        private static readonly HashSet<CaseStatus> PromotableToAssigned = new()
        {
            CaseStatus.New,
            CaseStatus.NewlyIdentified,
            CaseStatus.VerificationRequired,
            CaseStatus.NotAdvertised,
            CaseStatus.InProgress
        };

        public async Task AssignAsync(ComplianceCase @case, CancellationToken cancellationToken = default)
        {
            if (@case.AgentId.HasValue)
                return;

            var settings = await _settingsOrDefaultsAsync(cancellationToken);

            if (!settings.AutoAssignmentEnabled)
            {
                _logger.LogInformation(
                    "Auto-assignment disabled. Case {CaseId} left unassigned for manual allocation.",
                    @case.Id);

                await NotifyManagersManualAssignmentRequiredAsync(
                    @case,
                    "Automatic assignment is disabled in system settings.",
                    cancellationToken);

                return;
            }

            Agent? agent;

            try
            {
                agent = await _autoAssignmentService.SelectAgentAsync(
                    @case, settings.DistributionMethod, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Auto-assignment threw for case {CaseId}. Routing to manual.", @case.Id);

                await NotifyManagersManualAssignmentRequiredAsync(
                    @case,
                    $"Automatic assignment failed: {ex.Message}",
                    cancellationToken);

                return;
            }

            if (agent is null)
            {
                _logger.LogInformation(
                    "No eligible agent for case {CaseId}. Left unassigned for manual allocation.",
                    @case.Id);

                await NotifyManagersManualAssignmentRequiredAsync(
                    @case,
                    "No eligible agent was available (all at capacity or none active).",
                    cancellationToken);

                return;
            }

            @case.AgentId = agent.Id;
            @case.AssignedOn = DateTime.UtcNow;
            @case.ModifiedOn = DateTime.UtcNow;

            if (PromotableToAssigned.Contains(@case.Status))
                @case.Status = CaseStatus.Assigned;

            await _caseRepository.UpdateAsync(@case, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.Assigned,
                AuditEntity.ComplianceCase,
                @case.Id,
                $"Case automatically assigned to {agent.FullName}.",
                null,
                cancellationToken);

            var tenderRef = @case.Tender?.TenderNumber ?? @case.TenderId.ToString();

            await _notificationService.NotifyAsync(new CreateNotificationModel
            {
                UserId = agent.UserId,
                Title = "New Case Assigned",
                Message = $"Tender {tenderRef} has been assigned to you.",
                Type = NotificationType.Information,
                Url = $"/cases/{@case.Id}"
            }, cancellationToken);
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
            @case.ModifiedBy = managerUserId;
            @case.ModifiedOn = DateTime.UtcNow;

            if (PromotableToAssigned.Contains(@case.Status))
                @case.Status = CaseStatus.Assigned;

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

            var tenderRef = @case.Tender?.TenderNumber ?? @case.TenderId.ToString();

            await _notificationService.NotifyAsync(new CreateNotificationModel
            {
                UserId = agent.UserId,
                Title = "New Case Assigned",
                Message = $"Tender {tenderRef} has been assigned to you.",
                Type = NotificationType.Information,
                Url = $"/cases/{@case.Id}"
            }, cancellationToken);
        }

        private async Task<SystemSettingModel> _settingsOrDefaultsAsync(CancellationToken cancellationToken)
        {
            return await _systemSettingService.GetAsync();
        }

        /// <summary>
        /// Public so callers (e.g. ComplianceProcessingService) can trigger the same
        /// manager notification when assignment is skipped entirely.
        /// </summary>
        public async Task NotifyManagersManualAssignmentRequiredAsync(
            ComplianceCase @case,
            string reason,
            CancellationToken cancellationToken = default)
        {
            var managerIds = await _userService
                .GetManagerUserIdsAsync(cancellationToken);

            if (managerIds.Count == 0)
            {
                _logger.LogWarning(
                    "No manager or administrator users found to notify about " +
                    "manual assignment for case {CaseId}.", @case.Id);
                return;
            }

            var tenderRef = @case.Tender?.TenderNumber ?? @case.TenderId.ToString();

            foreach (var managerId in managerIds)
            {
                await _notificationService.NotifyAsync(new CreateNotificationModel
                {
                    UserId = managerId,
                    Title = "Manual Assignment Required",
                    Message =
                        $"Case {@case.Id} for tender {tenderRef} could not be auto-assigned. " +
                        $"Reason: {reason}. Please assign it manually.",
                    Type = NotificationType.Warning,
                    Url = $"/cases/{@case.Id}"
                }, cancellationToken);
            }
        }
    }
}