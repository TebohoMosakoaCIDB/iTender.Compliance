using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface IComplianceActionRepository
    {
        Task<ComplianceAction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<ComplianceAction>> GetByCaseIdAsync(Guid caseId, CancellationToken cancellationToken = default);
        Task AddAsync(ComplianceAction action, CancellationToken cancellationToken = default);
        Task UpdateAsync(ComplianceAction action, CancellationToken cancellationToken = default);
        Task DeleteAsync(ComplianceAction action, CancellationToken cancellationToken = default);
    }
}