using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class LeastWorkloadStrategy : ICaseDistributionStrategy
    {
        private readonly IAgentRepository _agentRepository;
        private readonly IComplianceCaseRepository _caseRepository;
        public CaseDistributionMethod Method => CaseDistributionMethod.LeastWorkload;
        public LeastWorkloadStrategy(
            IAgentRepository agentRepository,
            IComplianceCaseRepository caseRepository)
        {
            _agentRepository = agentRepository;
            _caseRepository = caseRepository;
        }

        public async Task<Agent?> SelectAgentAsync(ComplianceCase complianceCase, CancellationToken cancellationToken = default)
        {
            var agents = await _agentRepository
                .GetActiveAsync(cancellationToken);

            Agent? selectedAgent = null;
            var lowestWorkload = int.MaxValue;

            foreach (var agent in agents.Where(a => a.AutoAssignEnabled))
            {
                var workload = await _caseRepository
                    .GetOpenCaseCountByAgentAsync(
                        agent.Id,
                        cancellationToken);

                if (workload >= agent.MaximumOpenCases)
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
