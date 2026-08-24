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

        public AgentLevel? Level { get; set; }
        public string? JobTitle { get; set; }
        public string? HeaderImagePath { get; set; }
        public string? SignatureImagePath { get; set; }
        public string? FooterText { get; set; }
    }


    public class FindingDto
    {
        public Guid Id { get; set; }
        public ComplianceStream Stream { get; set; }
        public ComplianceFindingType FindingType { get; set; }
        public string Description { get; set; } = string.Empty;
        public string RegulatoryReference { get; set; } = string.Empty;
        public DateTime IdentifiedAt { get; set; }
        public bool IsResolved { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public TenderStatus TenderStatusAtCheck { get; set; }
    }

    public class ActionDto
    {
        public Guid Id { get; set; }
        public ComplianceActionType ActionType { get; set; }
        public ComplianceActionStatus Status { get; set; }
        public DateTime ActionDate { get; set; }
        public DateTime? ResponseDueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Comments { get; set; }
    }
}
