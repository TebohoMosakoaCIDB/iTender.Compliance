namespace iTender.Compliance.Application.Models.Reports
{
    public class ComplianceReportModel
    {
        public int TotalCases { get; set; }

        public int CompliantCases { get; set; }

        public int NonCompliantCases { get; set; }

        public double CompliancePercentage { get; set; }

        public double NonCompliancePercentage { get; set; }
    }
}
