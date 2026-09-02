using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using iTender.Compliance.Application.DTOs;
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

        public async Task<List<Tender>> GetUnregisteredAwardedTendersAsync(
            int minimumDaysSinceClosing,
            CancellationToken cancellationToken = default)
        {
            var cutoff = DateTime.UtcNow.AddDays(-minimumDaysSinceClosing);

            return await Context.Tenders
                .Where(t => t.AwardedDate != null && !t.IsRegisteredOnRoP)
                .Where(t => t.ClosingDate <= cutoff)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            Tender tender,
            CancellationToken cancellationToken = default)
        {
            await Context.Tenders.AddAsync(tender, cancellationToken);
        }

        public async Task<Tender?> GetDetailAsync(
    Guid id,
    CancellationToken cancellationToken = default)
        {
            return await Context.Tenders
                .AsNoTracking()

                .Include(x => x.TenderSync)
                    .ThenInclude(x => x.Logs)

                .Include(x => x.ComplianceCase)

                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
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

        public Task<List<Tender>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Context.Tenders
                .AsNoTracking()
                .Include(x => x.ComplianceCase)
                .OrderByDescending(x => x.AdvertisedDate)
                .ToListAsync(cancellationToken);
        }

        public Task<Tender?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
        {
            return Context.Tenders
                .AsNoTracking()

                .Include(x => x.TenderSync)

                .Include(x => x.ComplianceCase)
                    .ThenInclude(x => x.Agent)

                .Include(x => x.ComplianceCase)
                    .ThenInclude(x => x.ComplianceFindings)

                .Include(x => x.ComplianceCase)
                    .ThenInclude(x => x.ComplianceActions)

                .Include(x => x.ComplianceCase)
                    .ThenInclude(x => x.CaseLetters)

                .Include(x => x.ComplianceCase)
                    .ThenInclude(x => x.CaseNotes)

                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
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

        public async Task<PagedResult<Tender>> SearchAsync(TenderSearchModel search, CancellationToken cancellationToken = default)
        {
            var query = Context.Tenders
                .Include(x => x.TenderSync)
                .AsNoTracking()
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(search.SearchText))
            {
                var searchText = search.SearchText.Trim();

                query = query.Where(x =>
                    x.TenderNumber.Contains(searchText) ||
                    x.Title.Contains(searchText) ||
                    x.EmployerName.Contains(searchText) ||
                    (x.Description != null &&
                     x.Description.Contains(searchText)));
            }

            // Source
            if (!string.IsNullOrWhiteSpace(search.Source))
            {
                query = query.Where(x =>
                    x.Source == search.Source);
            }

            // Construction
            if (search.IsConstruction.HasValue)
            {
                query = query.Where(x =>
                    x.IsConstruction == search.IsConstruction.Value);
            }

            // RoP registration
            if (search.IsRegisteredOnRoP.HasValue)
            {
                query = query.Where(x =>
                    x.IsRegisteredOnRoP == search.IsRegisteredOnRoP.Value);
            }

            // Awarded
            if (search.HasBeenAwarded.HasValue)
            {
                if (search.HasBeenAwarded.Value)
                {
                    query = query.Where(x =>
                        x.AwardedDate.HasValue);
                }
                else
                {
                    query = query.Where(x =>
                        !x.AwardedDate.HasValue);
                }
            }

            // Advertised date
            if (search.FromAdvertisedDate.HasValue)
            {
                query = query.Where(x =>
                    x.AdvertisedDate >= search.FromAdvertisedDate.Value);
            }

            if (search.ToAdvertisedDate.HasValue)
            {
                var toDate = search.ToAdvertisedDate.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.AdvertisedDate < toDate);
            }

            // Closing date
            if (search.FromClosingDate.HasValue)
            {
                query = query.Where(x =>
                    x.ClosingDate >= search.FromClosingDate.Value);
            }

            if (search.ToClosingDate.HasValue)
            {
                var toDate = search.ToClosingDate.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.ClosingDate < toDate);
            }

            if (search.HasComplianceCase.HasValue)
            {
                if (search.HasComplianceCase.Value)
                {
                    query = query.Where(x =>
                        x.ComplianceCase != null);
                }
                else
                {
                    query = query.Where(x =>
                        x.ComplianceCase == null);
                }
            }

            // Total before paging
            var totalCount =
                await query.CountAsync(cancellationToken);

            // Paging
            var items = await query
                .OrderByDescending(x => x.AdvertisedDate)
                .Skip((search.PageNumber - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Tender>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = search.PageNumber,
                PageSize = search.PageSize
            };
        }
    }
}