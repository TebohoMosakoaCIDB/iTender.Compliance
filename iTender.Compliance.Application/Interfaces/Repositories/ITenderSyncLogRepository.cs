using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface ITenderSyncLogRepository
    {
        Task AddAsync(
            TenderSyncLog log,
            CancellationToken cancellationToken = default);

        Task<List<TenderSyncLog>> GetBySyncIdAsync(
            Guid tenderSyncId,
            CancellationToken cancellationToken = default);

        Task<TenderSyncDetailModel?> GetDetailAsync(
    Guid id,
    CancellationToken cancellationToken = default);
    }
}
