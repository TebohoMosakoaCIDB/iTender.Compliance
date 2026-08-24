using iTender.Compliance.Application.DTOs;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IReportService
    {
        Task<ReportSummaryModel> GetReportAsync(
        DateTime fromDate,
        DateTime toDate);

        Task<List<ReportTenderModel>> GetNonCompliantTendersAsync(
            DateTime fromDate,
            DateTime toDate);
    }
}
