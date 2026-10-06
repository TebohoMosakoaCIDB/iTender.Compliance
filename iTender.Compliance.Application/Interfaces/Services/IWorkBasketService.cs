using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IWorkBasketService
    {
        Task<IReadOnlyList<ComplianceCase>> GetCasesAsync(
            WorkBasket basket,
            Guid? agentId = null,
            CancellationToken cancellationToken = default);

        Task<Dictionary<WorkBasket, int>> GetCountsAsync(
            Guid? agentId = null,
            CancellationToken cancellationToken = default);
    }
}
