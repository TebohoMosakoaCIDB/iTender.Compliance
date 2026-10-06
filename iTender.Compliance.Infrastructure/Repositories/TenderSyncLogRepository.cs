using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class TenderSyncLogRepository : RepositoryBase, ITenderSyncLogRepository
    {
        public TenderSyncLogRepository(IDbContextFactory<ComplianceDbContext> contextFactory)
            : base(contextFactory)
        {
        }

        public async Task AddAsync(
            TenderSyncLog log,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            await context.SyncLogs.AddAsync(log, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<TenderSyncLog>> GetBySyncIdAsync(
            Guid tenderSyncId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.SyncLogs
                .Where(x => x.TenderSyncId == tenderSyncId)
                .OrderBy(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<TenderSyncDetailModel?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.TenderSyncs
                .Where(x => x.Id == id)
                .Select(x => new TenderSyncDetailModel
                {
                    Id = x.Id,
                    StartedOn = x.StartedOn,
                    CompletedOn = x.CompletedOn,
                    IsManual = x.IsManual,
                    TotalRetrieved = x.TotalRetrieved,
                    TotalCompliant = x.TotalCompliant,
                    TotalNonCompliant = x.TotalNonCompliant,
                    CasesCreated = x.CasesCreated,
                    ErrorCount = x.ErrorCount,

                    Logs = x.Logs
                        .OrderBy(l => l.CreatedOn)
                        .Select(l => new TenderSyncLogModel
                        {
                            Date = l.CreatedOn,
                            Type = l.Type,
                            Level = l.Level,
                            TenderNumber = l.TenderNumber,
                            Title = l.Title,
                            Message = l.Message
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}