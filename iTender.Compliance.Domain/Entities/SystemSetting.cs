using iTender.Compliance.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
