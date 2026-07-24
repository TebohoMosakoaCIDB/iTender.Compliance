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
    }
}
