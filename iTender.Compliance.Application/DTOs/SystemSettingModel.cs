using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class SystemSettingModel
    {
        public int ResponseDueHours { get; set; }

        public int ReminderDelayHours { get; set; }

        public int SynchronizationIntervalHours { get; set; }

        public int MaximumReminders { get; set; }

        public int DefaultPageSize { get; set; }

        public bool EnableAutomaticReminders { get; set; } = true;

        public int ReminderAfterHours { get; set; } = 48;

        public int ReminderCheckIntervalMinutes { get; set; } = 60;

        public bool AutoAssignmentEnabled { get; set; }

        public CaseDistributionMethod DistributionMethod { get; set; }

        public int OpenTenderResponseHours { get; set; } = 48;

        public int ClosedTenderResponseDays { get; set; } = 14;

        public int ContraventionNoticeResponseDays { get; set; } = 14;

        public bool RequireManagerApproval { get; set; } = true;
    }
}
