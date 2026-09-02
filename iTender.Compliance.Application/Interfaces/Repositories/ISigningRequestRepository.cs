using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface ISigningRequestRepository
    {
        Task AddAsync(
            SigningRequest request,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            SigningRequest request,
            CancellationToken cancellationToken = default);

        Task<SigningRequest?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<SigningRequest?> GetByCaseLetterAsync(
            Guid caseLetterId,
            CancellationToken cancellationToken = default);

        Task<List<SigningRequest>> GetPendingAsync(
            CancellationToken cancellationToken = default);

        /// <summary>Most recent signing requests of any status, with the case letter, compliance
        /// case and tender loaded - for the Manager-facing Approvals page.</summary>
        Task<List<SigningRequest>> GetRecentWithDetailsAsync(
            int take,
            CancellationToken cancellationToken = default);
    }
}