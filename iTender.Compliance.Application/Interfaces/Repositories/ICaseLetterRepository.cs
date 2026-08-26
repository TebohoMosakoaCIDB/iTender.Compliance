using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface ICaseLetterRepository
    {
        Task<List<CaseLetter>> GetByComplianceCaseIdAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default);
        Task<CaseLetter?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<List<CaseLetter>> GetByCaseIdAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default);

        Task<CaseLetter?> GetLatestAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default);

        Task<List<CaseLetter>> GetOutstandingAsync(
            CancellationToken cancellationToken = default);

        /// <summary>Outstanding (no response, past due) letters with the parent case, tender and agent loaded - for escalation processing.</summary>
        Task<List<CaseLetter>> GetOutstandingWithCaseAsync(
            CancellationToken cancellationToken = default);

        Task AddAsync(
            CaseLetter letter,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            CaseLetter letter,
            CancellationToken cancellationToken = default);
    }
}