using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class RoundRobinStrategy : ICaseDistributionStrategy
    {
        private readonly IAgentRepository _agentRepository;

        private static int _lastAssignedIndex = -1;

        public CaseDistributionMethod Method =>
            CaseDistributionMethod.RoundRobin;


        public RoundRobinStrategy(
            IAgentRepository agentRepository)
        {
            _agentRepository = agentRepository;
        }


        public async Task<Agent?> SelectAgentAsync(
            ComplianceCase complianceCase,
            CancellationToken cancellationToken = default)
        {
            var agents = (await _agentRepository
                    .GetActiveAsync(cancellationToken))
                .Where(x => x.AutoAssignEnabled)
                .OrderBy(x => x.Id)
                .ToList();


            if (!agents.Any())
                return null;


            _lastAssignedIndex++;

            if (_lastAssignedIndex >= agents.Count)
                _lastAssignedIndex = 0;


            return agents[_lastAssignedIndex];
        }
    }
}
