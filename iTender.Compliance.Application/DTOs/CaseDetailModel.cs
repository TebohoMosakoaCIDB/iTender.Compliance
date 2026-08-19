using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class CaseDetailModel
    {
        public string Status { get; set; } = string.Empty;

        public CasePriority Priority { get; set; }

        public string? Outcome { get; set; }

        public string? Agent { get; set; }
        public string? AgentEmail { get; set; }
        public Guid? AgentId { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ClosedOn { get; set; }

        public string? Comments { get; set; }

        public AgentLevel Level { get; set; }
        public string? JobTitle { get; set; }
        public string? HeaderImagePath { get; set; }
        public string? SignatureImagePath { get; set; }
        public string? FooterText { get; set; }
    }
}
