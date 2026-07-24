namespace iTender.Compliance.Application.DTOs
{
    public class SendReminderLetterModel
    {
        public Guid ComplianceCaseId { get; set; }

        public Guid CaseLetterId { get; set; }

        #region Recipient

        public string RecipientName { get; set; } = string.Empty;

        public string RecipientEmail { get; set; } = string.Empty;

        #endregion

        #region Tender

        public string TenderNumber { get; set; } = string.Empty;

        public string TenderTitle { get; set; } = string.Empty;

        public string EmployerName { get; set; } = string.Empty;

        public DateTime ClosingDate { get; set; }

        #endregion

        #region Reminder

        public DateTime OriginalSentOn { get; set; }

        public DateTime ResponseDueOn { get; set; }

        public int ReminderNumber { get; set; } = 1;

        #endregion
    }
}
