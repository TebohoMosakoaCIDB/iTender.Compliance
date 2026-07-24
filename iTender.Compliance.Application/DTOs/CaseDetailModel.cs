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

        public DateTime CreatedOn { get; set; }

        public DateTime? ClosedOn { get; set; }

        public string? Comments { get; set; }
    }
}
