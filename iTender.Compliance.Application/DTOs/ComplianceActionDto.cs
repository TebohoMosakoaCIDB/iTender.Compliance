using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class ComplianceActionDto
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
