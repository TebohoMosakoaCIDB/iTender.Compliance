using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class AgsaReferralRepository : RepositoryBase, IAgsaReferralRepository
    {
        public AgsaReferralRepository(ComplianceDbContext context)
            : base(context)
        {
        }

        public Task<AGSAReferral?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Context.AGSAReferrals
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<AGSAReferral?> GetByCaseIdAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default)
        {
            return Context.AGSAReferrals
                .FirstOrDefaultAsync(
                    x => x.ComplianceCaseId == complianceCaseId,
                    cancellationToken);
        }

        public Task<List<AGSAReferral>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Context.AGSAReferrals
                .OrderByDescending(x => x.ReferralDate)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            AGSAReferral referral,
            CancellationToken cancellationToken = default)
        {
            await Context.AGSAReferrals.AddAsync(referral, cancellationToken);
        }

        public Task UpdateAsync(
            AGSAReferral referral,
            CancellationToken cancellationToken = default)
        {
            Context.AGSAReferrals.Update(referral);
            return Task.CompletedTask;
        }
    }
}