using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Services
{
    public class AutoAssignmentService : IAutoAssignmentService
    {
        private readonly LeastWorkloadStrategy _leastWorkloadStrategy;
        private readonly PriorityBasedAssignmentStrategy _priorityBasedStrategy;
        private readonly RandomStrategy _randomStrategy;
        private readonly RoundRobinStrategy _roundRobinStrategy;
        private readonly ISystemSettingRepository _systemSettingRepository;
        private readonly IComplianceCaseRepository _complianceCaseRepository;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ComplianceDbContext _context;

        public AutoAssignmentService(
            LeastWorkloadStrategy leastWorkloadStrategy,
            PriorityBasedAssignmentStrategy priorityBasedStrategy,
            ISystemSettingRepository systemSettingRepository,
            IComplianceCaseRepository complianceCaseRepository,
            INotificationService notificationService,
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            ComplianceDbContext context,
            RandomStrategy randomStrategy,
            RoundRobinStrategy roundRobinStrategy)
        {
            _leastWorkloadStrategy = leastWorkloadStrategy;
            _priorityBasedStrategy = priorityBasedStrategy;
            _complianceCaseRepository = complianceCaseRepository;
            _systemSettingRepository = systemSettingRepository;
            _roundRobinStrategy = roundRobinStrategy;
            _notificationService = notificationService;
            _randomStrategy = randomStrategy;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task AssignUnassignedCasesAsync(CancellationToken cancellationToken = default)
        {
            var settings = await _systemSettingRepository
                .GetAsync(cancellationToken);

            if (!settings.AutoAssignmentEnabled)
                return;

            var cases = await GetUnassignedCasesAsync(cancellationToken);

            foreach (var complianceCase in cases)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var agent = await SelectAgentAsync(
                    complianceCase,
                    settings.DistributionMethod,
                    cancellationToken);

                if (agent == null)
                    continue;

                complianceCase.AgentId = agent.Id;
                complianceCase.AssignedOn = DateTime.UtcNow;
                complianceCase.Status = CaseStatus.Assigned;

                await _complianceCaseRepository.UpdateAsync(
                    complianceCase,
                    cancellationToken);

                await _auditService.LogAsync(
                    AuditAction.Assigned,
                    AuditEntity.ComplianceCase,
                    complianceCase.Id,
                    $"Case automatically assigned to {agent.FullName}.",
                    null,
                    cancellationToken);

                await _notificationService.NotifyAsync(new CreateNotificationModel
                {
                    UserId = agent.UserId,
                    Title = "New Case Assigned",
                    Message = $"Tender {complianceCase.Tender.TenderNumber} has been assigned to you.",
                    Type = NotificationType.Information,
                    Url = $"/cases/{complianceCase.Id}"
                });
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<ComplianceCase>> GetUnassignedCasesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ComplianceCases
                .Include(x => x.Tender)
                .Where(x =>
                    x.AgentId == null &&
                    x.Status == CaseStatus.New)
                .ToListAsync(cancellationToken);
        }

        public async Task<Agent?> SelectAgentAsync(
            ComplianceCase complianceCase,
            CaseDistributionMethod method,
            CancellationToken cancellationToken = default)
        {
            return method switch
            {
                CaseDistributionMethod.LeastWorkload =>
                    await _leastWorkloadStrategy.SelectAgentAsync(
                        complianceCase,
                        cancellationToken),

                CaseDistributionMethod.PriorityBased =>
                    await _priorityBasedStrategy.SelectAgentAsync(
                        complianceCase,
                        cancellationToken),

                CaseDistributionMethod.Random =>
                    await _randomStrategy.SelectAgentAsync(
                        complianceCase,
                        cancellationToken),

                CaseDistributionMethod.RoundRobin =>
                    await _roundRobinStrategy.SelectAgentAsync(
                        complianceCase,
                        cancellationToken),

                _ => null
            };
        }
    }
}
