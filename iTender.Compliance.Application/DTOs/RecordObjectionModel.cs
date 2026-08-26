using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class RecordObjectionModel
    {
        public Guid ComplianceCaseId { get; set; }

        public Guid CaseLetterId { get; set; }

        public string Reason { get; set; } = string.Empty;

        public DateTime ReceivedOn { get; set; } = DateTime.UtcNow;
    }

    public class ResolveObjectionModel
    {
        public Guid ObjectionId { get; set; }

        public ObjectionDecision Decision { get; set; }

        public string? ManagerNotes { get; set; }
    }
}