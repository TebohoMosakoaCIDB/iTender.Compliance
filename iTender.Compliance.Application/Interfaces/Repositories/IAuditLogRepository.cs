using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface IAuditLogRepository
    {
        Task AddAsync(
            AuditLog auditLog,
            CancellationToken cancellationToken = default);

        Task<AuditLog?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<List<AuditLog>> GetByUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<List<AuditLog>> GetRecentAsync(
            int count,
            CancellationToken cancellationToken = default);

        Task<List<AuditLog>> GetAllAsync(
            CancellationToken cancellationToken = default);
    }
}
