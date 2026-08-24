using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class SystemSetting : BaseEntity
    {
        // Existing properties
        public int ResponseDueHours { get; set; } = 48;

        public int ReminderDelayHours { get; set; } = 48;

        public int SynchronizationIntervalHours { get; set; } = 12;

        public int MaximumReminders { get; set; } = 3;

        public int DefaultPageSize { get; set; } = 20;

        public bool EnableAutomaticReminders { get; set; } = true;

        public int ReminderAfterHours { get; set; } = 48;

        public int ReminderCheckIntervalMinutes { get; set; } = 60;

        public bool AutoAssignmentEnabled { get; set; }

        public CaseDistributionMethod DistributionMethod { get; set; }

        public int DefaultMaximumOpenCases { get; set; }

        public int LastInstructionalLetterNumber { get; set; }
        public int LastContraventionNoticeNumber { get; set; }

        // ---- NEW PROPERTIES FOR COMPLIANCE TIMELINES ----

        /// <summary>
        /// Number of hours allowed for the client to respond to an Instructional Letter (IL).
        /// Default: 48 hours (2 working days).
        /// </summary>
        public int InstructionalLetterResponseHours { get; set; } = 48;

        /// <summary>
        /// Number of days allowed for the client to respond to a Contravention Notice (CN).
        /// Default: 14 days.
        /// </summary>
        public int ContraventionNoticeResponseDays { get; set; } = 14;
    }
}