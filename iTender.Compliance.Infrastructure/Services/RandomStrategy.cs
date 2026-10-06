using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class RandomStrategy : ICaseDistributionStrategy
    {
        private readonly IAgentRepository _agentRepository;
        private readonly IClientGroupRepository _clientGroupRepository;
        private readonly Random _random = new();

        public CaseDistributionMethod Method => CaseDistributionMethod.Random;

        public RandomStrategy(
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

            if (agents.Count == 0)
                return null;

            var eligible = new List<Agent>();

            foreach (var agent in agents)
            {
                var workload = await _clientGroupRepository
                    .GetOpenGroupCountByAgentAsync(agent.Id, cancellationToken);

                if (workload < agent.MaximumOpenGroups)
                    eligible.Add(agent);
            }

            if (eligible.Count == 0)
                return null;

            var index = _random.Next(eligible.Count);

            return eligible[index];
        }
    }
}