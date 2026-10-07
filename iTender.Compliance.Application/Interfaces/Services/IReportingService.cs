using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Models.Reports;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IReportingService
    {
        Task<ComplianceAnalysisModel> GetComplianceAnalysisAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default);

        Task<MonthlyReportModel> GetMonthlyReportAsync(
            int year,
            int month,
            CancellationToken cancellationToken = default);

        Task<ComplianceReportModel> GetComplianceReportAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default);

        Task<List<AgentReportModel>> GetAgentReportAsync(
            CancellationToken cancellationToken = default);

        Task<SyncReportModel> GetSynchronizationReportAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default);

        Task<List<AuditReportModel>> GetAuditReportAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<LateRopRegistrationModel>> GetLateRopRegistrationsAsync(
            DateTime from, DateTime to, CancellationToken cancellationToken = default);

        Task<MonthlyReportModel> GetQuarterlyReportAsync(
            int year,
            int quarter,
            CancellationToken cancellationToken = default);
    }
}
