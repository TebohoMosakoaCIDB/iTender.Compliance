using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface ICaseObjectionRepository
    {
        Task<CaseObjection?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<List<CaseObjection>> GetByCaseIdAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default);

        Task<List<CaseObjection>> GetAwaitingReviewAsync(
            CancellationToken cancellationToken = default);

        Task AddAsync(
            CaseObjection objection,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            CaseObjection objection,
            CancellationToken cancellationToken = default);
    }
}