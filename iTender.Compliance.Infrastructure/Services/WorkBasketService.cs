using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class WorkBasketService : IWorkBasketService
    {
        private readonly IComplianceCaseRepository _caseRepository;

        public WorkBasketService(IComplianceCaseRepository caseRepository)
        {
            _caseRepository = caseRepository;
        }

        public Task<IReadOnlyList<ComplianceCase>> GetCasesAsync(
            WorkBasket basket,
            Guid? agentId = null,
            CancellationToken cancellationToken = default)
        {
            return _caseRepository.GetInBasketAsync(basket, agentId, cancellationToken);
        }

        public async Task<Dictionary<WorkBasket, int>> GetCountsAsync(
            Guid? agentId = null,
            CancellationToken cancellationToken = default)
        {
            var counts = new Dictionary<WorkBasket, int>();

            foreach (var basket in Enum.GetValues<WorkBasket>())
            {
                var cases = await _caseRepository
                    .GetInBasketAsync(basket, agentId, cancellationToken);

                counts[basket] = cases.Count;
            }

            return counts;
        }
    }
}