namespace iTender.Compliance.Application.Models.Reports
{
    public class AgentReportModel
    {
        public string AgentName { get; set; } = string.Empty;

        public int AssignedCases { get; set; }

        public int ClosedCases { get; set; }

        public int OutstandingCases { get; set; }
    }
}
