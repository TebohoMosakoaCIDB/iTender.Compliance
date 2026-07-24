using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class RandomStrategy : ICaseDistributionStrategy
    {
        private readonly IAgentRepository _agentRepository;

        private readonly Random _random = new();


        public CaseDistributionMethod Method =>
            CaseDistributionMethod.Random;


        public RandomStrategy(
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
                .ToList();


            if (!agents.Any())
                return null;


            var index = _random.Next(
                agents.Count);


            return agents[index];
        }
    }
}
