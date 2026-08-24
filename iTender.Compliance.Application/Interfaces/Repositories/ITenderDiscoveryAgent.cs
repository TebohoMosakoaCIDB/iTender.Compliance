using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface ITenderDiscoveryAgent
    {
        Task<IReadOnlyList<DiscoveredTenderDto>> FindTendersAsync(
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default);
    }
}
