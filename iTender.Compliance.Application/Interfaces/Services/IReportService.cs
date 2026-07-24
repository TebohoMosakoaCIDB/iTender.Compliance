using iTender.Compliance.Application.DTOs.Reports;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IReportService
    {
        Task<ComplianceSummaryReportModel> GetComplianceSummaryAsync(
            CancellationToken cancellationToken = default);

        Task<List<OutstandingCasesReportModel>> GetOutstandingCasesAsync(
            CancellationToken cancellationToken = default);

        Task<List<AgentPerformanceReportModel>> GetAgentPerformanceAsync(
            CancellationToken cancellationToken = default);

        Task<List<SynchronizationReportModel>> GetSynchronizationHistoryAsync(
            CancellationToken cancellationToken = default);

        Task<List<LetterHistoryReportModel>> GetLetterHistoryAsync(
            CancellationToken cancellationToken = default);

        Task<List<AuditReportModel>> GetAuditHistoryAsync(
            CancellationToken cancellationToken = default);
    }
}
