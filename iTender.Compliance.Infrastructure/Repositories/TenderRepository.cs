using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class TenderRepository
    : RepositoryBase, ITenderRepository
    {
        public TenderRepository(ComplianceDbContext context)
            : base(context)
        {
        }

        public async Task AddAsync(
            Tender tender,
            CancellationToken cancellationToken = default)
        {
            await Context.Tenders.AddAsync(tender, cancellationToken);
        }

        public Task UpdateAsync(
            Tender tender,
            CancellationToken cancellationToken = default)
        {
            Context.Tenders.Update(tender);

            return Task.CompletedTask;
        }

        public async Task DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var tender = await GetByIdAsync(id, cancellationToken);

            if (tender != null)
            {
                Context.Tenders.Remove(tender);
            }
        }

        public Task<bool> ExistsAsync(
            string tenderNumber,
            CancellationToken cancellationToken = default)
        {
            return Context.Tenders
                .AnyAsync(x => x.TenderNumber == tenderNumber, cancellationToken);
        }

        public Task<List<Tender>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Context.Tenders
                .AsNoTracking()
                .OrderByDescending(x => x.AdvertisedDate)
                .ToListAsync(cancellationToken);
        }

        public Task<Tender?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Context.Tenders
                .Include(x => x.ComplianceCase)
                .Include(x => x.TenderSync)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<Tender?> GetByTenderNumberAsync(
            string tenderNumber,
            CancellationToken cancellationToken = default)
        {
            return Context.Tenders
                .Include(x => x.ComplianceCase)
                .FirstOrDefaultAsync(
                    x => x.TenderNumber == tenderNumber,
                    cancellationToken);
        }

        public Task<List<Tender>> GetBySyncIdAsync(
            Guid tenderSyncId,
            CancellationToken cancellationToken = default)
        {
            return Context.Tenders
                .Where(x => x.TenderSyncId == tenderSyncId)
                .ToListAsync(cancellationToken);
        }
    }
}
