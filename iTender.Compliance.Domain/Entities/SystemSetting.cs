using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class SystemSetting : BaseEntity
    {
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

        /// <summary>Response window for open tenders (Instruction Letter / erratum). Framework default: 48 hours.</summary>
        public int OpenTenderResponseHours { get; set; } = 48;

        /// <summary>Response window for closed tenders receiving a Contravention Notice directly. Framework default: 14 working days.</summary>
        public int ClosedTenderResponseDays { get; set; } = 14;

        /// <summary>Response window once a Contravention Notice has been issued (including IL escalations). Framework default: 14 working days.</summary>
        public int ContraventionNoticeResponseDays { get; set; } = 14;

        /// <summary>Whether letters must be routed to a Manager for sign-off before being sent to the client.</summary>
        public bool RequireManagerApproval { get; set; } = true;
    }
}