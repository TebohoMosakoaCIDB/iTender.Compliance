using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class AuditLogModel
    {
        public DateTime Date { get; set; }

        public string User { get; set; } = string.Empty;

        public AuditAction Action { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}
