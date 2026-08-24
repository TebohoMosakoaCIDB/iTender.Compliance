using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Application.Models.Dashboard;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IComplianceCaseRepository _complianceCaseRepository;
        private readonly ITenderSyncRepository _tenderSyncRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IAgentRepository _agentRepository;

        public DashboardService(
            IComplianceCaseRepository complianceCaseRepository,
            ITenderSyncRepository tenderSyncRepository,
            IAuditLogRepository auditLogRepository,
            IAgentRepository agentRepository)
        {
            _complianceCaseRepository = complianceCaseRepository;
            _tenderSyncRepository = tenderSyncRepository;
            _auditLogRepository = auditLogRepository;
            _agentRepository = agentRepository;
        }


        public async Task<DashboardModel> GetDashboardAsync(CancellationToken cancellationToken = default)
        {
            return new DashboardModel
            {
                Cards = await GetCardsAsync(cancellationToken),
                StatusChart = await GetStatusChartAsync(cancellationToken),
                AgentWorkloads = await GetAgentWorkloadsAsync(cancellationToken),
                ComplianceTrend = await GetComplianceTrendAsync(cancellationToken),
                SyncHistory = await GetSyncHistoryAsync(cancellationToken),
                RecentActivities = await GetRecentActivitiesAsync(10, cancellationToken),
                PriorityChart = await GetPriorityChartAsync(cancellationToken),
                OutcomeChart = await GetOutcomeChartAsync(cancellationToken),
                Sync = await GetSyncSummaryAsync(cancellationToken)
            };
        }

        public async Task<DashboardOutcomeChart> GetOutcomeChartAsync(CancellationToken cancellationToken = default)
        {
            var cases = await _complianceCaseRepository.GetAllAsync(cancellationToken);

            return new DashboardOutcomeChart
            {
                Compliant = cases.Count(x =>
                    x.Outcome == ComplianceOutcome.Compliant),

                NonCompliant = cases.Count(x =>
                    x.Outcome == ComplianceOutcome.NonCompliant),

                Pending = cases.Count(x =>
                    !x.Outcome.HasValue)
            };
        }

        public async Task<DashboardCards> GetCardsAsync(CancellationToken cancellationToken = default)
        {
            var cases = await _complianceCaseRepository.GetAllAsync(cancellationToken);
            var syncs = await _tenderSyncRepository.GetAllAsync(cancellationToken);

            return new DashboardCards
            {
                TotalCases = cases.Count,

                NewCases = cases.Count(x =>
                    x.Status == CaseStatus.New),

                AssignedCases = cases.Count(x =>
                    x.AgentId != null),

                WaitingForResponse = cases.Count(x =>
                    x.Status == CaseStatus.AwaitingILResponse),

                Compliant = cases.Count(x =>
                    x.Outcome == ComplianceOutcome.Compliant),

                NonCompliant = cases.Count(x =>
                    x.Outcome == ComplianceOutcome.NonCompliant),

                UnassignedCases = cases.Count(x =>
                    x.AgentId == null),

                FailedSyncs = syncs.Count(x =>
                    x.Status == SyncStatus.Failed)
            };
        }

        public async Task<DashboardStatusChart> GetStatusChartAsync(CancellationToken cancellationToken = default)
        {
            var cases = await _complianceCaseRepository.GetAllAsync(cancellationToken);

            return new DashboardStatusChart
            {
                New = cases.Count(x =>
                    x.Status == CaseStatus.New),

                Assigned = cases.Count(x =>
                    x.Status == CaseStatus.Assigned),

                Waiting = cases.Count(x =>
                    x.Status == CaseStatus.AwaitingILResponse),

                Compliant = cases.Count(x =>
                    x.Outcome == ComplianceOutcome.Compliant),

                NonCompliant = cases.Count(x =>
                    x.Outcome == ComplianceOutcome.NonCompliant),

                Closed = cases.Count(x =>
                    x.Status == CaseStatus.Closed)
            };
        }

        public async Task<List<AgentWorkload>> GetAgentWorkloadsAsync(CancellationToken cancellationToken = default)
        {
            var agents = await _agentRepository.GetActiveAsync(cancellationToken);
            var cases = await _complianceCaseRepository.GetAllAsync(cancellationToken);

            return agents
                .Select(agent => new AgentWorkload
                {
                    AgentName = agent.FullName,

                    AssignedCases = cases.Count(x =>
                        x.AgentId == agent.Id),

                    PendingCases = cases.Count(x =>
                        x.AgentId == agent.Id &&
                        x.Status != CaseStatus.Closed)
                })
                .OrderByDescending(x => x.PendingCases)
                .ToList();
        }

        public async Task<List<MonthlyComplianceChart>> GetComplianceTrendAsync(CancellationToken cancellationToken = default)
        {
            var cases = await _complianceCaseRepository.GetAllAsync(cancellationToken);

            return cases
                .GroupBy(x => new
                {
                    x.CreatedOn.Year,
                    x.CreatedOn.Month
                })
                .OrderBy(x => x.Key.Year)
                .ThenBy(x => x.Key.Month)
                .Select(g => new MonthlyComplianceChart
                {
                    Month = new DateTime(
                        g.Key.Year,
                        g.Key.Month,
                        1).ToString("MMM yyyy"),

                    Compliant = g.Count(x =>
                        x.Outcome == ComplianceOutcome.Compliant),

                    NonCompliant = g.Count(x =>
                        x.Outcome == ComplianceOutcome.NonCompliant)
                })
                .ToList();
        }

        public async Task<List<SyncHistoryChart>> GetSyncHistoryAsync(
    CancellationToken cancellationToken = default)
        {
            var syncs = await _tenderSyncRepository.GetAllAsync(cancellationToken);

            return syncs
                .OrderByDescending(x => x.StartedOn)
                .Take(10)
                .OrderBy(x => x.StartedOn)
                .Select(x => new SyncHistoryChart
                {
                    Date = x.StartedOn.ToString("dd MMM"),

                    Retrieved = x.TotalRetrieved,

                    CasesCreated = x.CasesCreated,

                    Errors = x.ErrorCount,

                    Successful = x.Status == SyncStatus.Completed
                })
                .ToList();
        }

        public async Task<DashboardPriorityChart> GetPriorityChartAsync(
    CancellationToken cancellationToken = default)
        {
            var cases = await _complianceCaseRepository.GetAllAsync(cancellationToken);

            return new DashboardPriorityChart
            {
                Low = cases.Count(x => x.Priority == CasePriority.Low),

                Normal = cases.Count(x => x.Priority == CasePriority.Normal),

                High = cases.Count(x => x.Priority == CasePriority.High),

                Critical = cases.Count(x => x.Priority == CasePriority.Critical)
            };
        }

        public async Task<List<RecentActivity>> GetRecentActivitiesAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            var activities = await _auditLogRepository
                .GetRecentAsync(count, cancellationToken);

            return activities
                .Select(x => new RecentActivity
                {
                    Description = x.Description,
                    User = x.CreatedBy?.ToString() ?? "System",
                    Date = x.CreatedOn
                })
                .ToList();
        }

        public async Task<SyncSummary> GetSyncSummaryAsync(CancellationToken cancellationToken = default)
        {
            var sync = await _tenderSyncRepository.GetLatestAsync(cancellationToken);

            if (sync == null)
                return new SyncSummary();

            return new SyncSummary
            {
                LastSync = sync.CompletedOn,
                Status = sync.Status,
                TotalRetrieved = sync.TotalRetrieved,
                CasesCreated = sync.CasesCreated
            };
        }
    }
}
