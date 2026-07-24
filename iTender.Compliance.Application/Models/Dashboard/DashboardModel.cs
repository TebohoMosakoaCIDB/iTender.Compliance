using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.Models.Dashboard
{
    public class DashboardModel
    {
        public DashboardCards Cards { get; set; } = new();

        public DashboardStatusChart StatusChart { get; set; } = new();

        public DashboardOutcomeChart OutcomeChart { get; set; } = new();

        public DashboardPriorityChart PriorityChart { get; set; } = new();

        public List<AgentWorkload> AgentWorkloads { get; set; } = [];

        public List<MonthlyComplianceChart> ComplianceTrend { get; set; } = [];

        public List<SyncHistoryChart> SyncHistory { get; set; } = [];

        public List<RecentActivity> RecentActivities { get; set; } = [];

        public SyncSummary Sync { get; set; } = new();
    }

    public class AgentWorkload
    {
        public string AgentName { get; set; } = string.Empty;

        public int AssignedCases { get; set; }

        public int PendingCases { get; set; }

        public int ClosedCases { get; set; }
    }

    public class SyncSummary
    {
        public DateTime? LastSync { get; set; }

        public SyncStatus? Status { get; set; }

        public int TotalRetrieved { get; set; }

        public int CasesCreated { get; set; }
    }


    public class DashboardCards
    {
        public int OverdueCases { get; set; }
        public int TotalCases { get; set; }

        public int NewCases { get; set; }

        public int AssignedCases { get; set; }

        public int WaitingForResponse { get; set; }

        public int Compliant { get; set; }

        public int NonCompliant { get; set; }

        public int UnassignedCases { get; set; }

        public int FailedSyncs { get; set; }
    }

    public class DashboardOutcomeChart
    {
        public int Compliant { get; set; }

        public int NonCompliant { get; set; }

        public int Pending { get; set; }
    }

    public class DashboardStatusChart
    {
        public int New { get; set; }

        public int Assigned { get; set; }

        public int Waiting { get; set; }

        public int Compliant { get; set; }

        public int NonCompliant { get; set; }

        public int Closed { get; set; }
    }

    public class DashboardChart
    {
        public string Label { get; set; } = string.Empty;

        public int Value { get; set; }
    }


    public class RecentActivity
    {
        public string Description { get; set; } = string.Empty;

        public string User { get; set; } = string.Empty;

        public DateTime Date { get; set; }
    }

    public class DashboardPriorityChart
    {
        public int Low { get; set; }

        public int Normal { get; set; }

        public int High { get; set; }

        public int Critical { get; set; }
    }

    public class SyncHistoryChart
    {
        public string Date { get; set; } = string.Empty;

        public int Retrieved { get; set; }

        public int CasesCreated { get; set; }

        public int Errors { get; set; }

        public bool Successful { get; set; }
    }

    public class MonthlyComplianceChart
    {
        public string Month { get; set; } = string.Empty;

        public int Compliant { get; set; }

        public int NonCompliant { get; set; }
    }
}
