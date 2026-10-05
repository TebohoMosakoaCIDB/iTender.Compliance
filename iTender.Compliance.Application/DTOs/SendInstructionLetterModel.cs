namespace iTender.Compliance.Application.DTOs
{
    public class SendInstructionLetterModel
    {
        public Guid ComplianceCaseId { get; set; }

        #region Recipient

        /// <summary>Formal designation used in the address block and salutation,
        /// e.g. "The Head of Department". Defaults to that since it's the common case.</summary>
        public string RecipientTitle { get; set; } = "The Head of Department";

        /// <summary>Short salutation used after "Dear" - e.g. "HOD". If not set, falls back to RecipientTitle.</summary>
        public string? Salutation { get; set; }

        public string RecipientName { get; set; } = string.Empty;

        public string RecipientEmail { get; set; } = string.Empty;

        /// <summary>Optional postal/physical address lines for the formal addressee block
        /// (department name, street, city, postal code, one per line). If empty, the
        /// address block is simply omitted rather than left with blank lines.</summary>
        public List<string> RecipientAddressLines { get; set; } = new();

        /// <summary>Name shown on the "Attention:" line. Defaults to RecipientName when not set.</summary>
        public string? AttentionName { get; set; }

        /// <summary>Optional secondary recipients CC'd on the notice (e.g. internal
        /// department contacts) - shown on the "Cc:" line and actually CC'd on the email.</summary>
        public List<string> CcEmails { get; set; } = new();

        public string CompanyName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;

        public string? HeaderImagePath { get; set; }
        public string? SignatureImagePath { get; set; }
        public string? FooterText { get; set; }

        #endregion

        #region Tender

        public string TenderNumber { get; set; } = string.Empty;

        public string TenderTitle { get; set; } = string.Empty;

        public string EmployerName { get; set; } = string.Empty;

        public DateTime ClosingDate { get; set; }

        #endregion

        #region Notice

        public DateTime ResponseDueOn { get; set; }

        #endregion
    }
}
