namespace iTender.Compliance.Application.DTOs
{
    public class LateRopRegistrationModel
    {
        public Guid TenderId { get; set; }
        public string TenderNumber { get; set; } = string.Empty;
        public string TenderTitle { get; set; } = string.Empty;
        public string EmployerName { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }

        public DateTime ClosingDate { get; set; }
        public DateTime AwardedDate { get; set; }
        public decimal? AwardValue { get; set; }

        /// <summary>The date the project was actually registered on the Register of Projects.</summary>
        public DateTime RoPRegistrationDate { get; set; }

        /// <summary>Closing date + the configured 21-day grace period.</summary>
        public DateTime RegistrationDeadline { get; set; }

        /// <summary>How many calendar days late the registration was (registration date minus deadline).</summary>
        public int DaysLate { get; set; }
    }
}
