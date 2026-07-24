using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class PriorityBasedAssignmentStrategy : ICaseDistributionStrategy
    {
        private readonly IAgentRepository _agentRepository;
        private readonly IComplianceCaseRepository _caseRepository;

        public CaseDistributionMethod Method =>CaseDistributionMethod.PriorityBased;
        public PriorityBasedAssignmentStrategy(
            IAgentRepository agentRepository,
            IComplianceCaseRepository caseRepository)
        {
            _agentRepository = agentRepository;
            _caseRepository = caseRepository;
        }

        public async Task<Agent?> SelectAgentAsync(ComplianceCase complianceCase, CancellationToken cancellationToken = default)
        {
            var agents = (await _agentRepository.GetActiveAsync(cancellationToken))
                .Where(a => a.AutoAssignEnabled)
                .ToList();

            // Filter agents based on case priority
            agents = complianceCase.Priority switch
            {
                CasePriority.Critical =>
                    agents.Where(a => a.Level == AgentLevel.Senior).ToList(),

                CasePriority.High =>
                    agents.Where(a =>
                        a.Level == AgentLevel.Senior ||
                        a.Level == AgentLevel.Intermediate).ToList(),

                CasePriority.Normal =>
                    agents.Where(a =>
                        a.Level != AgentLevel.Junior ||
                        true).ToList(),

                CasePriority.Low =>
                    agents,

                _ => agents
            };

            if (!agents.Any())
                return null;

            Agent? selectedAgent = null;
            var lowestWorkload = int.MaxValue;

            foreach (var agent in agents)
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
