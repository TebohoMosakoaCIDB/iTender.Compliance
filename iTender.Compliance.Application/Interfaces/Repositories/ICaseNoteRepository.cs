using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface ICaseNoteRepository
    {
        Task<List<CaseNote>> GetByCaseIdAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            CaseNote note,
            CancellationToken cancellationToken = default);
    }
}
