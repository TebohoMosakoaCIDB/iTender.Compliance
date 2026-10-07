using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class SystemSettingModel
    {
        // Response Deadlines
        public int InstructionLetterResponseWorkingDays { get; set; } = 2;
        public int InstructionLetterReminderAfterWorkingDays { get; set; } = 1;
        public int ClosedTenderResponseDays { get; set; } = 14;
        public int ContraventionNoticeResponseDays { get; set; } = 14;

        // AGSA Referral & RoP
        public int AgsaReferralDeadlineDays { get; set; } = 30;
        public int RopCheckAfterDays { get; set; } = 90;
        public int RopRegistrationGraceDays { get; set; } = 21;
        public string AgsaReferralEmail { get; set; } = "";
        public string EnforcementUnitEmail { get; set; } = "";

        // Reminders & Escalation
        public bool EnableAutomaticReminders { get; set; } = true;
        public int ReminderAfterWorkingDays { get; set; } = 7;
        public int ReminderCheckIntervalMinutes { get; set; } = 60;

        // Scheduled Reports
        public bool SendMonthlyReportAutomatically { get; set; } = false;
        public string? MonthlyReportRecipientEmail { get; set; }
        public int MonthlyReportHourUtc { get; set; } = 6;
        public string? LastMonthlyReportSent { get; set; }

        // Approval Workflow
        public bool RequireManagerApproval { get; set; } = true;

        // Synchronization
        public int SynchronizationIntervalHours { get; set; } = 12;

        // Group Assignment
        public bool AutoAssignmentEnabled { get; set; } = false;
        public CaseDistributionMethod DistributionMethod { get; set; }

        // Pagination
        public int DefaultPageSize { get; set; } = 20;

        // Existing fields you may also have (kept for backward compat)
        public int ResponseDueHours { get; set; } = 48;
        public int ReminderDelayHours { get; set; } = 48;
        public int MaximumReminders { get; set; } = 3;
        public int ReminderAfterHours { get; set; } = 48;
        public int DefaultMaximumOpenCases { get; set; }
        public int SlaWarningDays { get; set; } = 25;
    }
}
