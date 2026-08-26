namespace iTender.Compliance.Application.DTOs
{
    public class RequestExtensionModel
    {
        public Guid ComplianceCaseId { get; set; }

        public int AdditionalDays { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}