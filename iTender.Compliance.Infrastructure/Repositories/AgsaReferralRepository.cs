using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class AgsaReferralRepository : RepositoryBase, IAgsaReferralRepository
    {
        public AgsaReferralRepository(IDbContextFactory<ComplianceDbContext> contextFactory)
            : base(contextFactory)
        {
        }
        
        public async Task<int> CountByStatusAsync(
            EnforcementReferralStatus status,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.AGSAReferrals
                .CountAsync(r => r.Status == status, cancellationToken);
        }

        public async Task<AGSAReferral?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.AGSAReferrals
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<AGSAReferral?> GetByCaseIdAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.AGSAReferrals
                .FirstOrDefaultAsync(
                    x => x.ComplianceCaseId == complianceCaseId,
                    cancellationToken);
        }

        public async Task<List<AGSAReferral>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.AGSAReferrals
                .OrderByDescending(x => x.ReferralDate)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            AGSAReferral referral,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            await context.AGSAReferrals.AddAsync(referral, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(
            AGSAReferral referral,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            context.AGSAReferrals.Update(referral);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}