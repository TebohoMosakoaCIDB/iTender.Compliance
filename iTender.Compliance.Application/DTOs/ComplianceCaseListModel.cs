namespace iTender.Compliance.Application.DTOs
{
    public class ComplianceCaseListModel
    {
        public Guid Id { get; set; }

        public string TenderNumber { get; set; } = string.Empty;

        public string TenderTitle { get; set; } = string.Empty;

        public string Employer { get; set; } = string.Empty;

        public DateTime ClosingDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string? AssignedAgent { get; set; }
    }
}
