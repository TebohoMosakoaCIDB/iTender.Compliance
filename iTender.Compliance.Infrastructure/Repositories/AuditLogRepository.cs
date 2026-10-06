using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class AuditLogRepository : RepositoryBase, IAuditLogRepository
    {
        public AuditLogRepository(IDbContextFactory<ComplianceDbContext> contextFactory)
            : base(contextFactory)
        {
        }

        public async Task AddAsync(
            AuditLog auditLog,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            await context.AuditLogs.AddAsync(auditLog, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<AuditLog>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.AuditLogs
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<AuditLog?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.AuditLogs
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<AuditLog>> GetByUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.AuditLogs
                .Where(x => x.CreatedBy == userId)
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<AuditLog>> GetRecentAsync(
            int count,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.AuditLogs
                .OrderByDescending(x => x.CreatedOn)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
    }
}