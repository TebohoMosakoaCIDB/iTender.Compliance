using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class ComplianceAction : BaseEntity
    {
        public Guid ComplianceCaseId { get; set; }

        public ComplianceActionType ActionType { get; set; }

        public ComplianceActionStatus Status { get; set; }

        public DateTime ActionDate { get; set; }

        public DateTime? ResponseDueDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public string? Comments { get; set; }

        public virtual ComplianceCase ComplianceCase { get; set; } = null!;
    }
}
