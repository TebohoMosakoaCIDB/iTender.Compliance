using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface ITenderSyncRepository
    {
        Task<TenderSync?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<TenderSync?> GetLatestAsync(
            CancellationToken cancellationToken = default);

        Task<TenderSync?> GetRunningAsync(
            CancellationToken cancellationToken = default);

        Task<List<TenderSync>> GetAllAsync(
    CancellationToken cancellationToken = default);

        Task<PagedResult<TenderSync>> SearchAsync(
    TenderSyncSearchModel search,
    CancellationToken cancellationToken = default);

        Task AddAsync(
            TenderSync tenderSync,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            TenderSync tenderSync,
            CancellationToken cancellationToken = default);
    }
}
