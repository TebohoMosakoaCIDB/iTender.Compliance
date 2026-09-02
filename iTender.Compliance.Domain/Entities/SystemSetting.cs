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
        [Obsolete("Superseded by InstructionLetterResponseWorkingDays - working-day accurate. Kept for backward compatibility only, no longer read.")]
        public int OpenTenderResponseHours { get; set; } = 48;

        /// <summary>Response window for closed tenders receiving a Contravention Notice directly. Framework default: 14 working days.</summary>
        public int ClosedTenderResponseDays { get; set; } = 14;

        /// <summary>Response window once a Contravention Notice has been issued (including IL escalations). CIDB finalized rule: 14 WORKING days.</summary>
        public int ContraventionNoticeResponseDays { get; set; } = 14;

        /// <summary>Response window for an Instruction Letter on an open tender. CIDB finalized rule: 2 WORKING days.</summary>
        public int InstructionLetterResponseWorkingDays { get; set; } = 2;

        /// <summary>How many working days into the current outstanding letter's response window before a reminder is sent. CIDB finalized rule: day 7.</summary>
        public int ReminderAfterWorkingDays { get; set; } = 7;

        /// <summary>Absolute deadline, in calendar days from the date a case is allocated to a Compliance Officer, by which it must be
        /// resolved or referred for enforcement - independent of where it sits in the IL/CN cycle. CIDB finalized rule: 30 days.</summary>
        public int AgsaReferralDeadlineDays { get; set; } = 30;

        /// <summary>Days after a tender's closing date before RoP registration is checked at all - allows the procurement/award
        /// process to run its course. CIDB finalized rule: 90 days.</summary>
        public int RopCheckAfterDays { get; set; } = 90;

        /// <summary>Further grace period, in calendar days after RopCheckAfterDays, before an unregistered award becomes a
        /// compliance matter. CIDB finalized rule: 21 days (so 111 days after closing in total).</summary>
        public int RopRegistrationGraceDays { get; set; } = 21;

        /// <summary>Recipient for enforcement referrals. CIDB finalized: cidbcontraventionnotice@agsa.co.za</summary>
        public string AgsaReferralEmail { get; set; } = "cidbcontraventionnotice@agsa.co.za";

        /// <summary>CC'd on enforcement referrals. CIDB finalized: the internal CIDB enforcement unit contact.</summary>
        public string EnforcementUnitEmail { get; set; } = "Yolandam@cidb.org.za";

        /// <summary>Whether letters must be routed to a Manager for sign-off before being sent to the client.</summary>
        public bool RequireManagerApproval { get; set; } = true;
    }
}