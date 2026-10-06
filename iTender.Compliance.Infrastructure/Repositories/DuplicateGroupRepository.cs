using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class DuplicateGroupRepository : IDuplicateGroupRepository
    {
        private readonly IDbContextFactory<ComplianceDbContext> _contextFactory;

        public DuplicateGroupRepository(IDbContextFactory<ComplianceDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task AddAsync(
            DuplicateGroup group,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            await context.DuplicateGroups.AddAsync(group, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(
            DuplicateGroup group,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            context.DuplicateGroups.Update(group);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<DuplicateGroup?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.DuplicateGroups
                .Include(x => x.Candidates)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<DuplicateGroup>> GetPendingForCoAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.DuplicateGroups
                .Include(x => x.Candidates)
                .Where(x => x.Status == DuplicateStatus.PendingCOVerification)
                .OrderBy(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> TenderIsInAnyPendingGroupAsync(
            Guid tenderId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.DuplicateGroups
                .Where(x => x.Status == DuplicateStatus.PendingCOVerification)
                .AnyAsync(x => x.Candidates.Any(c => c.TenderId == tenderId), cancellationToken);
        }
    }
}