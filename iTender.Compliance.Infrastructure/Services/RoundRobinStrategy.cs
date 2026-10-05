using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class RoundRobinStrategy : ICaseDistributionStrategy
    {
        private readonly IAgentRepository _agentRepository;
        private readonly ISystemSettingRepository _systemSettingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CaseDistributionMethod Method => CaseDistributionMethod.RoundRobin;

        public RoundRobinStrategy(
            IAgentRepository agentRepository,
            ISystemSettingRepository systemSettingRepository,
            IUnitOfWork unitOfWork)
        {
            _agentRepository = agentRepository;
            _systemSettingRepository = systemSettingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Agent?> SelectAgentAsync(
            ComplianceCase complianceCase,
            CancellationToken cancellationToken = default)
        {
            var agents = (await _agentRepository
                    .GetActiveAsync(cancellationToken))
                .Where(x => x.AutoAssignEnabled)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .ToList();

            if (agents.Count == 0)
                return null;

            var settings = await _systemSettingRepository
                .GetAsync(cancellationToken);

            var index = settings.RoundRobinCursor % agents.Count;
            if (index < 0) index = 0; // safety if cursor ever goes negative

            var selected = agents[index];

            settings.RoundRobinCursor = (index + 1) % agents.Count;

            await _systemSettingRepository.UpdateAsync(
                settings,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return selected;
        }
    }
}