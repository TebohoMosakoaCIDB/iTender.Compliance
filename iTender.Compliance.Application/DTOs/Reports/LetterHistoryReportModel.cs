using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs.Reports
{
    public class LetterHistoryReportModel
    {
        public string TenderNumber { get; set; } = string.Empty;

        public string RecipientName { get; set; } = string.Empty;

        public string RecipientEmail { get; set; } = string.Empty;

        public LetterType LetterType { get; set; }

        public int LetterNumber { get; set; }

        public DateTime SentOn { get; set; }

        public DateTime? RespondedOn { get; set; }

        public bool ResponseReceived { get; set; }
    }
}