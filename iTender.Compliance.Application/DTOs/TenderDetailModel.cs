namespace iTender.Compliance.Application.DTOs
{
    public class TenderDetailModel
    {
        public Guid Id { get; set; }

        public string TenderNumber { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Employer { get; set; } = string.Empty;

        public DateTime ClosingDate { get; set; }

        public string TenderUrl { get; set; } = string.Empty;
    }
}
