using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class CaseObjectionRepository : RepositoryBase, ICaseObjectionRepository
    {
        public CaseObjectionRepository(IDbContextFactory<ComplianceDbContext> contextFactory)
            : base(contextFactory)
        {
        }

        public async Task<CaseObjection?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.CaseObjections
                .Include(x => x.ComplianceCase)
                .Include(x => x.CaseLetter)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<CaseObjection>> GetByCaseIdAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.CaseObjections
                .Where(x => x.ComplianceCaseId == complianceCaseId)
                .Include(x => x.ComplianceCase)
                .Include(x => x.CaseLetter)
                .OrderByDescending(x => x.ReceivedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CaseObjection>> GetAwaitingReviewAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.CaseObjections
                .Include(x => x.ComplianceCase)
                    .ThenInclude(c => c.Tender)
                .Where(x => x.Status != ObjectionStatus.Resolved)
                .OrderBy(x => x.ReceivedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            CaseObjection objection,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            await context.CaseObjections.AddAsync(objection, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(
            CaseObjection objection,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            context.CaseObjections.Update(objection);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}