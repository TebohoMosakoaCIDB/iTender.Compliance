using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class ComplianceActionRepository : IComplianceActionRepository
    {
        private readonly IDbContextFactory<ComplianceDbContext> _contextFactory;

        public ComplianceActionRepository(IDbContextFactory<ComplianceDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<ComplianceAction?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.ComplianceActions
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<ComplianceAction>> GetByCaseIdAsync(
            Guid caseId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.ComplianceActions
                .Where(a => a.ComplianceCaseId == caseId)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            ComplianceAction action,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            await context.ComplianceActions.AddAsync(action, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(
            ComplianceAction action,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            context.ComplianceActions.Update(action);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(
            ComplianceAction action,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            context.ComplianceActions.Remove(action);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}