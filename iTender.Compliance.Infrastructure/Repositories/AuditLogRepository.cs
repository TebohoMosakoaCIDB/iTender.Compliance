using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class AuditLogRepository
    : RepositoryBase, IAuditLogRepository
    {
        public AuditLogRepository(
            ComplianceDbContext context)
            : base(context)
        {
        }

        public async Task AddAsync(
            AuditLog auditLog,
            CancellationToken cancellationToken = default)
        {
            await Context.AuditLogs.AddAsync(
                auditLog,
                cancellationToken);
        }

        public Task<List<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Context.AuditLogs
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public Task<AuditLog?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Context.AuditLogs
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        
        public Task<List<AuditLog>> GetByUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Context.AuditLogs
                .Where(x => x.CreatedBy == userId)
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public Task<List<AuditLog>> GetRecentAsync(
            int count,
            CancellationToken cancellationToken = default)
        {
            return Context.AuditLogs
                .OrderByDescending(x => x.CreatedOn)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
    }
}
