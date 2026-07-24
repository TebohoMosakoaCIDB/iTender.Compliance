using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public AuditAction Action { get; set; }

        public AuditEntity Entity { get; set; }

        public Guid EntityId { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}
