namespace iTender.Compliance.Application.DTOs
{
    public class SendContraventionNoticeModel
    {
        public Guid ComplianceCaseId { get; set; }

        #region Recipient

        /// <summary>Formal designation used in the address block and salutation,
        /// e.g. "Head of Department". Defaults to that since it's the common case.</summary>
        public string RecipientTitle { get; set; } = "Head of Department";

        public string RecipientName { get; set; } = string.Empty;

        public string RecipientEmail { get; set; } = string.Empty;

        /// <summary>Optional postal/physical address lines for the formal addressee block
        /// (department name, street, city, postal code, one per line). If empty, the
        /// address block is simply omitted rather than left with blank lines.</summary>
        public List<string> RecipientAddressLines { get; set; } = new();

        /// <summary>Name shown on the "Attention:" line. Defaults to RecipientName when not set.</summary>
        public string? AttentionName { get; set; }

        /// <summary>Optional secondary recipient CC'd on the notice (e.g. an internal
        /// communications address) - shown on the "Cc:" line and actually CC'd on the email.</summary>
        public string? CcEmail { get; set; }

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

        /// <summary>"No response to Instruction Letter" or "Closed tender without RoP registration", etc.</summary>
        public string Reason { get; set; } = string.Empty;

        public DateTime ResponseDueOn { get; set; }

        #endregion
    }
}