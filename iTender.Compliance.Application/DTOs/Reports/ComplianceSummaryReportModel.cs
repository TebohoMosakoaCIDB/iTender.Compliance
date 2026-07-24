namespace iTender.Compliance.Application.DTOs.Reports
{
    public class ComplianceSummaryReportModel
    {
        public DateTime GeneratedOn { get; set; }

        public int TotalCases { get; set; }

        public int NewCases { get; set; }

        public int AssignedCases { get; set; }

        public int WaitingForResponse { get; set; }

        public int CompliantCases { get; set; }

        public int NonCompliantCases { get; set; }

        public int ClosedCases { get; set; }

        public int UnassignedCases { get; set; }

        public double AverageResponseHours { get; set; }
    }
}
