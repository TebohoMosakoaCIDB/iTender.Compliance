using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class ComplianceFindingRepository : IComplianceFindingRepository
    {
        private readonly IDbContextFactory<ComplianceDbContext> _contextFactory;

        public ComplianceFindingRepository(IDbContextFactory<ComplianceDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<ComplianceFinding?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.ComplianceFindings
                .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<ComplianceFinding>> GetByCaseIdAsync(
            Guid caseId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.ComplianceFindings
                .Where(f => f.ComplianceCaseId == caseId)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            ComplianceFinding finding,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            await context.ComplianceFindings.AddAsync(finding, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(
            ComplianceFinding finding,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            context.ComplianceFindings.Update(finding);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(
            ComplianceFinding finding,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            context.ComplianceFindings.Remove(finding);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ComplianceFinding>> GetAwaitingCoReviewAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.ComplianceFindings
                .Include(f => f.ComplianceCase)
                    .ThenInclude(c => c.Tender)
                .Include(f => f.ComplianceCase)
                    .ThenInclude(c => c.Agent)
                .Where(f => f.VerificationStatus == VerificationStatus.AutomatedFailed
                         && !f.IsResolved)
                .OrderBy(f => f.IdentifiedAt)
                .ToListAsync(cancellationToken);
        }
    }
}