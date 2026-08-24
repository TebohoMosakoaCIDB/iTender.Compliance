using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface ITenderRepository
    {
        Task<List<Tender>> GetUnregisteredAwardedTendersAsync(CancellationToken cancellationToken = default);

        Task<Tender?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<Tender?> GetByTenderNumberAsync(
            string tenderNumber,
            CancellationToken cancellationToken = default);

        Task<List<Tender>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<List<Tender>> GetBySyncIdAsync(
            Guid tenderSyncId,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            string tenderNumber,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            Tender tender,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            Tender tender,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<Tender?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<PagedResult<Tender>> SearchAsync(
            TenderSearchModel search,
            CancellationToken cancellationToken = default);
    }
}
