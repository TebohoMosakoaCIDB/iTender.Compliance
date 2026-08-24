using iTender.Compliance.Application.DTOs;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface IReportRepository
    {
        Task<int> GetTotalTendersAsync(
            DateTime fromDate,
            DateTime toDate);

        Task<int> GetTotalCasesAsync(
            DateTime fromDate,
            DateTime toDate);

        Task<List<ReportTenderModel>> GetCasesAsync(
            DateTime fromDate,
            DateTime toDate);

        Task<List<CaseStatusSummaryModel>> GetStatusBreakdownAsync(
            DateTime fromDate,
            DateTime toDate);

        Task<List<ComplianceOutcomeSummaryModel>> GetOutcomeBreakdownAsync(
            DateTime fromDate,
            DateTime toDate);

        Task<List<ReportTenderModel>> GetNonCompliantTendersAsync(
            DateTime fromDate,
            DateTime toDate);
    }
}
