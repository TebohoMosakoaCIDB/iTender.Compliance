using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface IClientResponseRepository
    {
        Task<ClientResponse?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ClientResponse>> GetByCaseIdAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            ClientResponse response,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            ClientResponse response,
            CancellationToken cancellationToken = default);

        Task<bool> HasPendingReviewAsync(
            Guid caseLetterId,
            CancellationToken cancellationToken = default);

        Task<bool> HasResubmissionAsync(
            Guid previousResponseId,
            CancellationToken cancellationToken = default);

        Task<bool> HasAnyResponseAsync(
            Guid caseLetterId,
            CancellationToken cancellationToken = default);
    }
}