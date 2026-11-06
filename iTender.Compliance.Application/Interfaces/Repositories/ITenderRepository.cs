using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface ITenderRepository
    {
        Task<IReadOnlyList<Guid>> MarkClassificationReviewedAsync(
            IEnumerable<Guid> tenderIds,
            Guid userId,
            string? notes,
            CancellationToken cancellationToken = default);

        Task<int> CountConstructionTendersAsync(
            CancellationToken cancellationToken = default);

        Task<List<Tender>> GetUnregisteredAwardedTendersAsync(
            int minimumDaysSinceClosing,
            CancellationToken cancellationToken = default);

        Task<Tender?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<int> CountAwaitingClassificationReviewAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Tender>> GetByIdsAsync(
            IEnumerable<Guid> ids,
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

        // Add to ITenderRepository
        Task<IReadOnlyList<Tender>> GetByNormalizedTitleAsync(
            string normalizedTitle,
            Guid excludeTenderId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Tender>> GetByEmployerAndClosingDateAsync(
            string employerName,
            DateTime closingDate,
            Guid excludeTenderId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Tender>> GetLateRopRegistrationsAsync(
            DateTime from,
            DateTime to,
            int graceDays,
            CancellationToken cancellationToken = default);

        Task<PagedResult<Tender>> SearchForClassificationReviewAsync(
            TenderClassificationSearchModel search,
            CancellationToken cancellationToken = default);

    //    Task<int> CountAwaitingClassificationReviewAsync(
    //Guid? agentId = null,
    //CancellationToken cancellationToken = default);
    }
}