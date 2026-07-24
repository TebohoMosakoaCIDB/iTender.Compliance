namespace iTender.Compliance.Application.DTOs.Reports
{
    public class AgentPerformanceReportModel
    {
        public string AgentName { get; set; } = string.Empty;

        public int AssignedCases { get; set; }

        public int CompletedCases { get; set; }

        public int PendingCases { get; set; }

        public int CompliantCases { get; set; }

        public int NonCompliantCases { get; set; }

        public double AverageResponseHours { get; set; }
    }
}
