using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class ComplianceFinding : BaseEntity
    {
        public Guid ComplianceCaseId { get; set; }

        public ComplianceStream Stream { get; set; }

        public ComplianceFindingType FindingType { get; set; }

        public string Description { get; set; } = string.Empty;

        public string RegulatoryReference { get; set; } = string.Empty;

        public DateTime IdentifiedAt { get; set; }

        public bool IsResolved { get; set; }

        public DateTime? ResolvedOn { get; set; }
        public TenderStatus TenderStatusAtCheck { get; set; }

        public virtual ComplianceCase ComplianceCase { get; set; } = null!;
    }
}
