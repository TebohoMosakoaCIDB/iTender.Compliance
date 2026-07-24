using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class CaptureResponseModel
    {
        public Guid ComplianceCaseId { get; set; }

        public Guid CaseLetterId { get; set; }

        public ComplianceOutcome Outcome { get; set; }

        public DateTime RespondedOn { get; set; } = DateTime.UtcNow;

        public string? Comments { get; set; }
    }
}
