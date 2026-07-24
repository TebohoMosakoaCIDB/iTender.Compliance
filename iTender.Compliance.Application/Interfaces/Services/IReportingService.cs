using iTender.Compliance.Application.Models.Reports;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IReportingService
    {
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
    }
}
