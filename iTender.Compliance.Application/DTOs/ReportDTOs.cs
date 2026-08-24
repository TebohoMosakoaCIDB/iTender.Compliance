using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class ReportSummaryModel
    {
        public int TotalTenders { get; set; }

        public int TotalCases { get; set; }

        public int CasesOpened { get; set; }

        public int CasesClosed { get; set; }

        public int CasesInProgress { get; set; }

        public int CasesEscalated { get; set; }

        public int Compliant { get; set; }

        public int NonCompliant { get; set; }

        public decimal ComplianceRate { get; set; }

        public List<CaseStatusSummaryModel> StatusBreakdown { get; set; }
            = new();

        public List<ComplianceOutcomeSummaryModel> OutcomeBreakdown { get; set; }
            = new();
    }

    public class CaseStatusSummaryModel
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class ComplianceOutcomeSummaryModel
    {
        public string Outcome { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class ReportTenderModel
    {
        public Guid TenderId { get; set; }

        public string TenderNumber { get; set; } = string.Empty;

        public string ClientName { get; set; } = string.Empty;

        public DateTime AdvertisedDate { get; set; }

        public CaseStatus Status { get; set; }

        public ComplianceOutcome? Outcome { get; set; }

        public CasePriority Priority { get; set; }
    }

    public class ReportSummaryDto
    {
        public int TotalTenders { get; set; }
        public int Compliant { get; set; }
        public int NonCompliant { get; set; }
        public int ComplianceRate { get; set; }

        // Stream breakdown
        public int Stream1Total { get; set; }
        public int Stream1Compliant { get; set; }
        public int Stream1CompliantRate { get; set; }

        public int Stream2Total { get; set; }
        public int Stream2Compliant { get; set; }
        public int Stream2CompliantRate { get; set; }

        public int Stream3Total { get; set; }
        public int Stream3Compliant { get; set; }
        public int Stream3CompliantRate { get; set; }

        // Finding breakdown
        public List<FindingBreakdownDto> FindingBreakdown { get; set; } = new();

        // Case activity
        public int CasesOpened { get; set; }
        public int CasesClosed { get; set; }
        public int CasesInProgress { get; set; }
        public int CasesEscalated { get; set; }
    }

    public class FindingBreakdownDto
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
        public int Percentage { get; set; }
    }

    public class NonCompliantTenderDto
    {
        public Guid TenderId { get; set; }
        public string TenderNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string Stream { get; set; } = string.Empty;
        public string FindingType { get; set; } = string.Empty;
        public DateTime IdentifiedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusColor => Status switch
        {
            "Open" => "danger",
            "InProgress" => "warning",
            "Closed" => "success",
            _ => "secondary"
        };
    }

    public class ComplianceReportPdfModel
    {
        public string Title { get; set; } = "CRCIP Compliance Report";

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public DateTime GeneratedAt { get; set; }

        public ReportSummaryModel Summary { get; set; } = null!;

        public List<ReportTenderModel> NonCompliantTenders { get; set; } = new();
    }
}
