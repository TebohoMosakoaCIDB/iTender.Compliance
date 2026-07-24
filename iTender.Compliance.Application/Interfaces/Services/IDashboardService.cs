using iTender.Compliance.Application.Models.Dashboard;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<DashboardModel> GetDashboardAsync(
            CancellationToken cancellationToken = default);

        Task<DashboardCards> GetCardsAsync(
            CancellationToken cancellationToken = default);

        Task<DashboardOutcomeChart> GetOutcomeChartAsync(
            CancellationToken cancellationToken = default);

        Task<DashboardPriorityChart> GetPriorityChartAsync(
            CancellationToken cancellationToken = default);

        Task<DashboardStatusChart> GetStatusChartAsync(
            CancellationToken cancellationToken = default);

        Task<List<AgentWorkload>> GetAgentWorkloadsAsync(
            CancellationToken cancellationToken = default);

        Task<List<MonthlyComplianceChart>> GetComplianceTrendAsync(
            CancellationToken cancellationToken = default);

        Task<List<SyncHistoryChart>> GetSyncHistoryAsync(
            CancellationToken cancellationToken = default);

        Task<List<RecentActivity>> GetRecentActivitiesAsync(
            int count = 10,
            CancellationToken cancellationToken = default);

        Task<SyncSummary> GetSyncSummaryAsync(
            CancellationToken cancellationToken = default);
    }
}
