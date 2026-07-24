using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class ComplianceCaseSearchModel
    {
        public string? SearchText { get; set; }

        public Guid? AgentId { get; set; }

        public Guid? TenderId { get; set; }

        public CaseStatus? Status { get; set; }

        public ComplianceOutcome? Outcome { get; set; }

        public CasePriority? Priority { get; set; }

        public LetterType? LetterType { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public bool? AssignedOnly { get; set; }

        public bool? AwaitingResponse { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }
}
