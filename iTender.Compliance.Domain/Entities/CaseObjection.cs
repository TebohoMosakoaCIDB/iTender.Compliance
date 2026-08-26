using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class CaseObjection : BaseEntity
    {
        public Guid ComplianceCaseId { get; set; }

        public Guid CaseLetterId { get; set; }

        public DateTime ReceivedOn { get; set; }

        public string Reason { get; set; } = string.Empty;

        public ObjectionStatus Status { get; set; }

        public Guid? ReviewedByAgentId { get; set; }

        public DateTime? ReviewedOn { get; set; }

        public ObjectionDecision? Decision { get; set; }

        public string? ManagerNotes { get; set; }

        public virtual ComplianceCase ComplianceCase { get; set; } = null!;

        public virtual CaseLetter CaseLetter { get; set; } = null!;
    }
}
