using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Domain.Enums
{
    public class ClientGroupCase
    {
        public Guid Id { get; set; }
        public Guid ClientGroupId { get; set; }
        public Guid ComplianceCaseId { get; set; }
        public DateTime AddedOn { get; set; } = DateTime.UtcNow;

        public virtual ClientGroup ClientGroup { get; set; } = null!;
        public virtual ComplianceCase ComplianceCase { get; set; } = null!;
    }
}
