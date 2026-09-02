namespace iTender.Compliance.Application.DTOs
{
    public class SendContraventionNoticeModel
    {
        public Guid ComplianceCaseId { get; set; }

        #region Recipient

        public string RecipientName { get; set; } = string.Empty;

        public string RecipientEmail { get; set; } = string.Empty;

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