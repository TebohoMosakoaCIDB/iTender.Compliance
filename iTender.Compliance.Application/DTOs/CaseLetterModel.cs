using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class CaseLetterModel
    {
        public Guid Id { get; set; }
        public int LetterNumber { get; set; }

        public DateTime SentOn { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public DateTime ResponseDueOn { get; set; }

        public DateTime? RespondedOn { get; set; }

        public LetterType Type { get; set; }

        public bool? Accepted { get; set; }

        public string FileName { get; set; } = string.Empty;
    }
}
