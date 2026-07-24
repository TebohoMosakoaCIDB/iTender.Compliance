using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class TenderSyncLogRepository : RepositoryBase, ITenderSyncLogRepository
    {
        public TenderSyncLogRepository(
            ComplianceDbContext context)
            : base(context)
        {
        }

        public async Task AddAsync(
            TenderSyncLog log,
            CancellationToken cancellationToken = default)
        {
            await Context.SyncLogs.AddAsync(log, cancellationToken);
        }

        public Task<List<TenderSyncLog>> GetBySyncIdAsync(
            Guid tenderSyncId,
            CancellationToken cancellationToken = default)
        {
            return Context.SyncLogs
                .Where(x => x.TenderSyncId == tenderSyncId)
                .OrderBy(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<TenderSyncDetailModel?> GetDetailAsync(
    Guid id,
    CancellationToken cancellationToken = default)
        {
            return await Context.TenderSyncs
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
