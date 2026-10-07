namespace iTender.Compliance.Application.DTOs
{
    public class ComplianceAnalysisModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public int TotalCases { get; set; }
        public int TotalCompliant { get; set; }
        public int TotalNonCompliant { get; set; }
        public int TotalOpen { get; set; }

        public List<AnalysisByStatus> ByStatus { get; set; } = new();
        public List<AnalysisByOutcome> ByOutcome { get; set; } = new();
        public List<AnalysisByOfficer> ByOfficer { get; set; } = new();
        public List<AnalysisByProvince> ByProvince { get; set; } = new();
        public List<AnalysisByEmployer> ByEmployer { get; set; } = new();
        public List<AnalysisByMonth> ByMonth { get; set; } = new();
    }

    public class AnalysisByStatus
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class AnalysisByOutcome
    {
        public string Outcome { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class AnalysisByOfficer
    {
        public string OfficerName { get; set; } = string.Empty;
        public int TotalCases { get; set; }
        public int Resolved { get; set; }
        public int Outstanding { get; set; }
        public decimal ResolvedPercentage { get; set; }
    }

    public class AnalysisByProvince
    {
        public string Province { get; set; } = string.Empty;
        public int TotalCases { get; set; }
        public int Compliant { get; set; }
        public int NonCompliant { get; set; }
        public int Open { get; set; }
    }

    public class AnalysisByEmployer
    {
        public string EmployerName { get; set; } = string.Empty;
        public int TotalCases { get; set; }
        public int Compliant { get; set; }
        public int NonCompliant { get; set; }
        public int Open { get; set; }
        public decimal NonComplianceRate { get; set; }
    }

    public class AnalysisByMonth
    {
        public string Month { get; set; } = string.Empty;
        public int Opened { get; set; }
        public int Closed { get; set; }
        public int Compliant { get; set; }
        public int NonCompliant { get; set; }
    }
}