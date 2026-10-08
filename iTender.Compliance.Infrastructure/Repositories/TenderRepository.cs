using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class TenderRepository : RepositoryBase, ITenderRepository
    {
        public TenderRepository(IDbContextFactory<ComplianceDbContext> contextFactory)
            : base(contextFactory)
        {
        }

        public async Task<int> CountConstructionTendersAsync( CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Tenders
                .CountAsync(t => t.IsConstruction, cancellationToken);
        }

        public async Task<int> CountAwaitingClassificationReviewAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Tenders
                .CountAsync(t => t.ClassificationReviewedOn == null, cancellationToken);
        }

        public async Task<int> MarkClassificationReviewedAsync(
    IEnumerable<Guid> tenderIds,
    Guid userId,
    string? notes,
    CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var idList = tenderIds?.Distinct().ToList() ?? new List<Guid>();

            if (idList.Count == 0)
                return 0;

            var tenders = await context.Tenders
                .Where(t => idList.Contains(t.Id))
                .Where(t => t.ClassificationReviewedOn == null)
                .ToListAsync(cancellationToken);

            if (tenders.Count == 0)
                return 0;

            var now = DateTime.UtcNow;

            foreach (var tender in tenders)
            {
                tender.ClassificationReviewedByUserId = userId;
                tender.ClassificationReviewedOn = now;
                tender.ClassificationReviewNotes = notes;
                tender.ClassificationOverridden = false;
            }

            await context.SaveChangesAsync(cancellationToken);

            return tenders.Count;
        }

        public async Task<List<Tender>> GetUnregisteredAwardedTendersAsync(
            int minimumDaysSinceClosing,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var cutoff = DateTime.UtcNow.AddDays(-minimumDaysSinceClosing);

            return await context.Tenders
                .Where(t => t.AwardedDate != null && !t.IsRegisteredOnRoP)
                .Where(t => t.ClosingDate <= cutoff)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            Tender tender,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            await context.Tenders.AddAsync(tender, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Tender?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Tenders
                .AsNoTracking()
                .Include(x => x.TenderSync)
                    .ThenInclude(x => x.Logs)
                .Include(x => x.ComplianceCase)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task UpdateAsync(
            Tender tender,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            context.Tenders.Update(tender);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var tender = await context.Tenders
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (tender == null)
                return;

            context.Tenders.Remove(tender);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(
            string tenderNumber,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Tenders
                .AnyAsync(x => x.TenderNumber == tenderNumber, cancellationToken);
        }

        public async Task<List<Tender>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Tenders
                .Include(x => x.ComplianceCase)
                .OrderByDescending(x => x.AdvertisedDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<Tender?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Tenders
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
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<Tender?> GetByTenderNumberAsync(
            string tenderNumber,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Tenders
                .Include(x => x.ComplianceCase)
                .FirstOrDefaultAsync(x => x.TenderNumber == tenderNumber, cancellationToken);
        }

        public async Task<List<Tender>> GetBySyncIdAsync(
            Guid tenderSyncId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Tenders
                .Where(x => x.TenderSyncId == tenderSyncId)
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResult<Tender>> SearchAsync(
            TenderSearchModel search,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var query = context.Tenders
                .Include(x => x.TenderSync)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search.SearchText))
            {
                var searchText = search.SearchText.Trim();

                query = query.Where(x =>
                    x.TenderNumber.Contains(searchText) ||
                    x.Title.Contains(searchText) ||
                    x.EmployerName.Contains(searchText) ||
                    (x.Description != null && x.Description.Contains(searchText)));
            }

            if (!string.IsNullOrWhiteSpace(search.Source))
                query = query.Where(x => x.Source == search.Source);

            if (search.IsConstruction.HasValue)
                query = query.Where(x => x.IsConstruction == search.IsConstruction.Value);

            if (search.IsRegisteredOnRoP.HasValue)
                query = query.Where(x => x.IsRegisteredOnRoP == search.IsRegisteredOnRoP.Value);

            if (search.HasBeenAwarded.HasValue)
            {
                if (search.HasBeenAwarded.Value)
                    query = query.Where(x => x.AwardedDate.HasValue);
                else
                    query = query.Where(x => !x.AwardedDate.HasValue);
            }

            if (search.FromAdvertisedDate.HasValue)
                query = query.Where(x => x.AdvertisedDate >= search.FromAdvertisedDate.Value);

            if (search.ToAdvertisedDate.HasValue)
            {
                var toDate = search.ToAdvertisedDate.Value.Date.AddDays(1);
                query = query.Where(x => x.AdvertisedDate < toDate);
            }

            if (search.FromClosingDate.HasValue)
                query = query.Where(x => x.ClosingDate >= search.FromClosingDate.Value);

            if (search.ToClosingDate.HasValue)
            {
                var toDate = search.ToClosingDate.Value.Date.AddDays(1);
                query = query.Where(x => x.ClosingDate < toDate);
            }

            if (search.HasComplianceCase.HasValue)
            {
                if (search.HasComplianceCase.Value)
                    query = query.Where(x => x.ComplianceCase != null);
                else
                    query = query.Where(x => x.ComplianceCase == null);
            }

            var totalCount = await query.CountAsync(cancellationToken);

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

        public async Task<IReadOnlyList<Tender>> GetByNormalizedTitleAsync(
            string normalizedTitle,
            Guid excludeTenderId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Tenders
                .Where(x => x.Id != excludeTenderId
                            && x.NormalizedTitle == normalizedTitle)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Tender>> GetByEmployerAndClosingDateAsync(
            string employerName,
            DateTime closingDate,
            Guid excludeTenderId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var day = closingDate.Date;

            return await context.Tenders
                .Where(x => x.Id != excludeTenderId
                            && x.EmployerName == employerName
                            && x.ClosingDate.Date == day)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Tender>> GetLateRopRegistrationsAsync(
            DateTime from,
            DateTime to,
            int graceDays,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Tenders
                .Where(t => t.IsRegisteredOnRoP
                         && t.RoPRegistrationDate != null
                         && t.RoPRegistrationDate > t.ClosingDate.AddDays(graceDays)
                         && t.RoPRegistrationDate >= from
                         && t.RoPRegistrationDate <= to)
                .OrderByDescending(t => t.RoPRegistrationDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResult<Tender>> SearchForClassificationReviewAsync(
            TenderClassificationSearchModel search,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var query = context.Tenders.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search.SearchText))
            {
                query = query.Where(t =>
                    t.TenderNumber.Contains(search.SearchText) ||
                    t.Title.Contains(search.SearchText) ||
                    t.EmployerName.Contains(search.SearchText));
            }

            if (search.IsConstruction.HasValue)
                query = query.Where(t => t.IsConstruction == search.IsConstruction.Value);

            if (search.ReviewedOnly == true)
                query = query.Where(t => t.ClassificationReviewedOn != null);
            else if (search.ReviewedOnly == false)
                query = query.Where(t => t.ClassificationReviewedOn == null);

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(t => t.CreatedOn)
                .Skip((search.PageNumber - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Tender>
            {
                Items = items,
                TotalCount = total,
                PageNumber = search.PageNumber,
                PageSize = search.PageSize
            };
        }

        public async Task<IReadOnlyList<Tender>> GetByIdsAsync(
            IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var idList = ids?.Distinct().ToList() ?? new List<Guid>();

            if (idList.Count == 0)
                return Array.Empty<Tender>();

            return await context.Tenders
                .Where(t => idList.Contains(t.Id))
                .ToListAsync(cancellationToken);
        }
    }
}