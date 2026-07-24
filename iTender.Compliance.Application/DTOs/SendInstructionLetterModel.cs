namespace iTender.Compliance.Application.DTOs
{
    public class SendInstructionLetterModel
    {
        public Guid ComplianceCaseId { get; set; }

        #region Recipient

        public string RecipientName { get; set; } = string.Empty;

        public string RecipientEmail { get; set; } = string.Empty;

        #endregion

        #region Tender

        public string TenderNumber { get; set; } = string.Empty;

        public string TenderTitle { get; set; } = string.Empty;

        public string EmployerName { get; set; } = string.Empty;

        public DateTime ClosingDate { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string ContactPerson { get; set; } = string.Empty;


        #endregion

        #region Letter

        public string Subject { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime ResponseDueOn { get; set; } = DateTime.UtcNow.AddHours(48);

        #endregion
    }
}
