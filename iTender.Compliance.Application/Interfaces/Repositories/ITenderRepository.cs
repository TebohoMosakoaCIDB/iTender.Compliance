using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface ITenderRepository
    {
        /// <summary>Awarded tenders still not registered on the RoP, at least minimumDaysSinceClosing days
        /// past their closing date (CIDB rule: 90-day procurement window + 21-day registration grace = 111).</summary>
        Task<List<Tender>> GetUnregisteredAwardedTendersAsync(
            int minimumDaysSinceClosing,
            CancellationToken cancellationToken = default);

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