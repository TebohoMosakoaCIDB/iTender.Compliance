using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface IComplianceFindingRepository
    {
        Task<ComplianceFinding?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<ComplianceFinding>> GetByCaseIdAsync(Guid caseId, CancellationToken cancellationToken = default);
        Task AddAsync(ComplianceFinding finding, CancellationToken cancellationToken = default);
        Task UpdateAsync(ComplianceFinding finding, CancellationToken cancellationToken = default);
        Task DeleteAsync(ComplianceFinding finding, CancellationToken cancellationToken = default);
    }
}