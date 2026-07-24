using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs.Reports
{
    public class OutstandingCasesReportModel
    {
        public string TenderNumber { get; set; } = string.Empty;

        public string TenderTitle { get; set; } = string.Empty;

        public string Employer { get; set; } = string.Empty;

        public string? AssignedAgent { get; set; }

        public CasePriority Priority { get; set; }

        public CaseStatus Status { get; set; }

        public DateTime CreatedOn { get; set; }

        public int DaysOpen { get; set; }
    }
}
