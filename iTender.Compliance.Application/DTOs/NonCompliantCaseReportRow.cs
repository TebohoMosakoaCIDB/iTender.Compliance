using iTender.Compliance.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace iTender.Compliance.Application.DTOs
{
    public class NonCompliantCaseReportRow
    {
        public Guid CaseId { get; set; }
        public string TenderNumber { get; set; } = string.Empty;
        public string TenderTitle { get; set; } = string.Empty;
        public string EmployerName { get; set; } = string.Empty;
        public string? Province { get; set; }

        public Guid? AgentId { get; set; }
        public string? AgentName { get; set; }

        public CaseStatus Status { get; set; }
        public CasePriority Priority { get; set; }

        public int FindingsCount { get; set; }
        public string FindingStreams { get; set; } = string.Empty; // "Class of Works, Advertisement"

        public DateTime CreatedOn { get; set; }
        public DateTime? AssignedOn { get; set; }
        public int DaysOpen { get; set; }

        public bool IsPastSla { get; set; }
        public bool IsSlaWarning { get; set; }

        public string? LastLetterType { get; set; }
        public DateTime? LastLetterSentOn { get; set; }
        public DateTime? ResponseDueOn { get; set; }

        public int? DaysUntilDue { get; set; }   // negative if ove
    }
}
