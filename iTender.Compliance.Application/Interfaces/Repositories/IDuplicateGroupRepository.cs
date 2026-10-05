using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface IDuplicateGroupRepository
    {
        Task AddAsync(DuplicateGroup group, CancellationToken cancellationToken = default);
        Task UpdateAsync(DuplicateGroup group, CancellationToken cancellationToken = default);
        Task<DuplicateGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<DuplicateGroup>> GetPendingForCoAsync(CancellationToken cancellationToken = default);
        Task<bool> TenderIsInAnyPendingGroupAsync(Guid tenderId, CancellationToken cancellationToken = default);
    }
}
