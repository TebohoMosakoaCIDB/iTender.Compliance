using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class ReportRepository : RepositoryBase, IReportRepository
    {
        public ReportRepository(ComplianceDbContext context) : base(context)
        {
        }

        public async Task<int> GetTotalTendersAsync(
            DateTime fromDate,
            DateTime toDate)
        {
            return await Context.Tenders
                .AsNoTracking()
                .Where(x =>
                    x.AdvertisedDate >= fromDate &&
                    x.AdvertisedDate <= toDate)
                .CountAsync();
        }

        public async Task<int> GetTotalCasesAsync(
            DateTime fromDate,
            DateTime toDate)
        {
            return await Context.ComplianceCases
                .AsNoTracking()
                .Where(x =>
                    x.CreatedOn >= fromDate &&
                    x.CreatedOn <= toDate)
                .CountAsync();
        }

        public async Task<List<ReportTenderModel>> GetCasesAsync(
            DateTime fromDate,
            DateTime toDate)
        {
            return await Context.ComplianceCases
                .AsNoTracking()
                .Where(x =>
                    x.CreatedOn >= fromDate &&
                    x.CreatedOn <= toDate)
                .Select(x => new ReportTenderModel
                {
                    TenderId = x.TenderId,

                    TenderNumber = x.Tender.TenderNumber,

                    ClientName = x.Tender.EmployerName,

                    AdvertisedDate = x.Tender.AdvertisedDate,

                    Status = x.Status,

                    Outcome = x.Outcome,

                    Priority = x.Priority
                })
                .ToListAsync();
        }

        public async Task<List<CaseStatusSummaryModel>> GetStatusBreakdownAsync(
            DateTime fromDate,
            DateTime toDate)
        {
            var results = await Context.ComplianceCases
                .AsNoTracking()
                .Where(x =>
                    x.CreatedOn >= fromDate &&
                    x.CreatedOn <= toDate)
                .GroupBy(x => x.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var total = results.Sum(x => x.Count);

            return results
                .Select(x => new CaseStatusSummaryModel
                {
                    Status = x.Status.ToString(),
                    Count = x.Count,
                    Percentage = total == 0
                        ? 0
                        : Math.Round((decimal)x.Count / total * 100, 2)
                })
                .OrderByDescending(x => x.Count)
                .ToList();
        }

        public async Task<List<ComplianceOutcomeSummaryModel>> GetOutcomeBreakdownAsync(
            DateTime fromDate,
            DateTime toDate)
        {
            var results = await Context.ComplianceCases
                .AsNoTracking()
                .Where(x =>
                    x.CreatedOn >= fromDate &&
                    x.CreatedOn <= toDate &&
                    x.Outcome.HasValue)
                .GroupBy(x => x.Outcome)
                .Select(g => new
                {
                    Outcome = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var total = results.Sum(x => x.Count);

            return results
                .Select(x => new ComplianceOutcomeSummaryModel
                {
                    Outcome = x.Outcome!.Value.ToString(),
                    Count = x.Count,
                    Percentage = total == 0
                        ? 0
                        : Math.Round((decimal)x.Count / total * 100, 2)
                })
                .OrderByDescending(x => x.Count)
                .ToList();
        }

        public async Task<List<ReportTenderModel>> GetNonCompliantTendersAsync(
            DateTime fromDate,
            DateTime toDate)
        {
            return await Context.ComplianceCases
                .AsNoTracking()
                .Where(x =>
                    x.CreatedOn >= fromDate &&
                    x.CreatedOn <= toDate &&
                    x.Outcome == ComplianceOutcome.NonCompliant)
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => new ReportTenderModel
                {
                    TenderId = x.TenderId,

                    TenderNumber = x.Tender.TenderNumber,

                    ClientName = x.Tender.EmployerName,

                    AdvertisedDate = x.Tender.AdvertisedDate,

                    Status = x.Status,

                    Outcome = x.Outcome,

                    Priority = x.Priority
                })
                .ToListAsync();
        }
    }
}
