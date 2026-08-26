namespace iTender.Compliance.Application.DTOs
{
    public class AgsaReferralDocumentModel
    {
        public string ReferralNumber { get; set; } = string.Empty;

        public string TenderNumber { get; set; } = string.Empty;

        public string TenderTitle { get; set; } = string.Empty;

        public string EmployerName { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime ReferralDate { get; set; }
    }
}