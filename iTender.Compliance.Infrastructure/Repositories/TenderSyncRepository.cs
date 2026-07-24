using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class TenderSyncRepository
    : RepositoryBase, ITenderSyncRepository
    {
        public TenderSyncRepository(
            ComplianceDbContext context)
            : base(context)
        {
        }

        public async Task AddAsync(
            TenderSync tenderSync,
            CancellationToken cancellationToken = default)
        {
            await Context.TenderSyncs.AddAsync(
                tenderSync,
                cancellationToken);
        }

        public Task UpdateAsync(
            TenderSync tenderSync,
            CancellationToken cancellationToken = default)
        {
            Context.TenderSyncs.Update(tenderSync);

            return Task.CompletedTask;
        }

        public async Task<List<TenderSync>> GetAllAsync(
    CancellationToken cancellationToken = default)
        {
            return await Context.TenderSyncs
                .OrderByDescending(x => x.StartedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResult<TenderSync>> SearchAsync(
    TenderSyncSearchModel search,
    CancellationToken cancellationToken = default)
        {
            var query = Context.TenderSyncs.AsQueryable();

            if (search.Status.HasValue)
            {
                query = query.Where(x => x.Status == search.Status.Value);
            }

            if (search.IsManual.HasValue)
            {
                query = query.Where(x => x.IsManual == search.IsManual.Value);
            }

            if (search.FromDate.HasValue)
            {
                query = query.Where(x => x.StartedOn >= search.FromDate.Value);
            }

            if (search.ToDate.HasValue)
            {
                var toDate = search.ToDate.Value.Date.AddDays(1);

                query = query.Where(x => x.StartedOn < toDate);
            }

            query = query.OrderByDescending(x => x.StartedOn);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((search.PageNumber - 1) * search.PageSize)
                .Take(search.PageSize)
                .Select(x => new TenderSync
                {
                    Id = x.Id,
                    StartedOn = x.StartedOn,
                    CompletedOn = x.CompletedOn,
                    IsManual = x.IsManual,
                    Status = x.Status,
                    TotalRetrieved = x.TotalRetrieved,
                    TotalCompliant = x.TotalCompliant,
                    TotalNonCompliant = x.TotalNonCompliant,
                    CasesCreated = x.CasesCreated,
                    ErrorCount = x.ErrorCount
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<TenderSync>
            {
                Items = items,
                PageNumber = search.PageNumber,
                PageSize = search.PageSize,
                TotalCount = totalCount
            };
        }

        public Task<TenderSync?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Context.TenderSyncs
                .Include(x => x.Tenders)
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        public Task<TenderSync?> GetLatestAsync(
            CancellationToken cancellationToken = default)
        {
            return Context.TenderSyncs
                .OrderByDescending(x => x.StartedOn)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<TenderSync?> GetRunningAsync(
            CancellationToken cancellationToken = default)
        {
            return Context.TenderSyncs
                .FirstOrDefaultAsync(
                    x => x.Status == SyncStatus.Running,
                    cancellationToken);
        }
    }
}
