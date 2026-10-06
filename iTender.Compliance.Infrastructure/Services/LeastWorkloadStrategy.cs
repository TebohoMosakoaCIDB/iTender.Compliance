using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class LeastWorkloadStrategy : ICaseDistributionStrategy
    {
        private readonly IAgentRepository _agentRepository;
        private readonly IClientGroupRepository _clientGroupRepository;
        public CaseDistributionMethod Method => CaseDistributionMethod.LeastWorkload;
        public LeastWorkloadStrategy(
            IAgentRepository agentRepository,
            IClientGroupRepository clientGroupRepository)
        {
            _agentRepository = agentRepository;
            _clientGroupRepository = clientGroupRepository;
        }

        public async Task<Agent?> SelectAgentAsync(
            ComplianceCase complianceCase,
            CancellationToken cancellationToken = default)
        {
            var agents = (await _agentRepository.GetActiveAsync(cancellationToken))
                .Where(a => a.AutoAssignEnabled)
                .ToList();

            Agent? selectedAgent = null;
            var lowestWorkload = int.MaxValue;

            foreach (var agent in agents)
            {
                var workload = await _clientGroupRepository
                    .GetOpenGroupCountByAgentAsync(agent.Id, cancellationToken);

                if (workload >= agent.MaximumOpenGroups)
                    continue;

                if (workload < lowestWorkload)
                {
                    lowestWorkload = workload;
                    selectedAgent = agent;
                }
            }

            return selectedAgent;
        }
    }
}
