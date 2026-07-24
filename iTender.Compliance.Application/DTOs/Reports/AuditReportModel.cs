using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs.Reports
{
    public class AuditReportModel
    {
        public DateTime Date { get; set; }

        public string User { get; set; } = string.Empty;

        public AuditAction Action { get; set; }

        public AuditEntity Entity { get; set; }

        public Guid EntityId { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}