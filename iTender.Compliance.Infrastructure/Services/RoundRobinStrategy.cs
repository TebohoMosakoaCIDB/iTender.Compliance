using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class RoundRobinStrategy : ICaseDistributionStrategy
    {
        private readonly IAgentRepository _agentRepository;
        private readonly IClientGroupRepository _clientGroupRepository;
        private readonly ISystemSettingRepository _systemSettingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CaseDistributionMethod Method => CaseDistributionMethod.RoundRobin;

        public RoundRobinStrategy(
            IAgentRepository agentRepository,
            IClientGroupRepository clientGroupRepository,
            ISystemSettingRepository systemSettingRepository,
            IUnitOfWork unitOfWork)
        {
            _agentRepository = agentRepository;
            _clientGroupRepository = clientGroupRepository;
            _systemSettingRepository = systemSettingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Agent?> SelectAgentAsync(
            ComplianceCase complianceCase,
            CancellationToken cancellationToken = default)
        {
            var allAgents = (await _agentRepository.GetActiveAsync(cancellationToken))
                .Where(a => a.AutoAssignEnabled)
                .OrderBy(a => a.DisplayOrder)
                .ThenBy(a => a.Id)
                .ToList();

            if (allAgents.Count == 0)
                return null;

            // Filter out agents at their open-group cap.
            var eligible = new List<Agent>();

            foreach (var agent in allAgents)
            {
                var workload = await _clientGroupRepository
                    .GetOpenGroupCountByAgentAsync(agent.Id, cancellationToken);

                if (workload < agent.MaximumOpenGroups)
                    eligible.Add(agent);
            }

            if (eligible.Count == 0)
                return null;

            var settings = await _systemSettingRepository.GetAsync(cancellationToken);

            var index = settings.RoundRobinCursor % eligible.Count;
            if (index < 0) index = 0;

            var selected = eligible[index];

            settings.RoundRobinCursor = (index + 1) % eligible.Count;

            await _systemSettingRepository.UpdateAsync(settings, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return selected;
        }
    }
}