using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;

namespace iTender.Compliance.Infrastructure.Services
{
    public class SystemSettingService
    : ISystemSettingService
    {
        private readonly ISystemSettingRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public SystemSettingService(
            ISystemSettingRepository repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<SystemSettingModel> GetAsync()
        {
            var settings = await _repository.GetAsync();

            return new SystemSettingModel
            {
                // Response Deadlines
                ResponseDueHours = settings.ResponseDueHours,
                ReminderDelayHours = settings.ReminderDelayHours,
                ClosedTenderResponseDays = settings.ClosedTenderResponseDays,
                ContraventionNoticeResponseDays = settings.ContraventionNoticeResponseDays,
                InstructionLetterResponseWorkingDays = settings.InstructionLetterResponseWorkingDays,

                // Reminders & Escalation
                EnableAutomaticReminders = settings.EnableAutomaticReminders,
                ReminderAfterWorkingDays = settings.ReminderAfterWorkingDays,
                InstructionLetterReminderAfterWorkingDays = settings.InstructionLetterReminderAfterWorkingDays,
                ReminderCheckIntervalMinutes = settings.ReminderCheckIntervalMinutes,
                MaximumReminders = settings.MaximumReminders,
                SlaWarningDays = settings.SlaWarningDays,

                // AGSA Referral & RoP
                AgsaReferralDeadlineDays = settings.AgsaReferralDeadlineDays,
                RopCheckAfterDays = settings.RopCheckAfterDays,
                RopRegistrationGraceDays = settings.RopRegistrationGraceDays,
                AgsaReferralEmail = settings.AgsaReferralEmail,
                EnforcementUnitEmail = settings.EnforcementUnitEmail,

                // Approval Workflow
                RequireManagerApproval = settings.RequireManagerApproval,

                // Synchronization
                SynchronizationIntervalHours = settings.SynchronizationIntervalHours,

                // Scheduled Reports
                SendMonthlyReportAutomatically = settings.SendMonthlyReportAutomatically,
                MonthlyReportRecipientEmail = settings.MonthlyReportRecipientEmail,
                MonthlyReportHourUtc = settings.MonthlyReportHourUtc,
                LastMonthlyReportSent = settings.LastMonthlyReportSent,

                // Group Assignment
                AutoAssignmentEnabled = settings.AutoAssignmentEnabled,
                DistributionMethod = settings.DistributionMethod,
                DefaultMaximumOpenCases = settings.DefaultMaximumOpenCases,

                // Pagination
                DefaultPageSize = settings.DefaultPageSize
            };
        }

        public async Task SaveAsync(SystemSettingModel model)
        {
            var settings = await _repository.GetAsync();

            // ---------------------------------------------------
            // Deadlines
            // ---------------------------------------------------
            settings.ResponseDueHours = model.ResponseDueHours;
            settings.ReminderDelayHours = model.ReminderDelayHours;
            settings.ClosedTenderResponseDays = model.ClosedTenderResponseDays;
            settings.ContraventionNoticeResponseDays = model.ContraventionNoticeResponseDays;
            settings.InstructionLetterResponseWorkingDays = model.InstructionLetterResponseWorkingDays;

            // ---------------------------------------------------
            // Reminders & Escalation
            // ---------------------------------------------------
            settings.EnableAutomaticReminders = model.EnableAutomaticReminders;
            settings.ReminderAfterWorkingDays = model.ReminderAfterWorkingDays;
            settings.InstructionLetterReminderAfterWorkingDays = model.InstructionLetterReminderAfterWorkingDays;
            settings.ReminderCheckIntervalMinutes = model.ReminderCheckIntervalMinutes;
            settings.MaximumReminders = model.MaximumReminders;
            settings.SlaWarningDays = model.SlaWarningDays;

            // ---------------------------------------------------
            // AGSA Referral & RoP
            // ---------------------------------------------------
            settings.AgsaReferralDeadlineDays = model.AgsaReferralDeadlineDays;
            settings.RopCheckAfterDays = model.RopCheckAfterDays;
            settings.RopRegistrationGraceDays = model.RopRegistrationGraceDays;
            settings.AgsaReferralEmail = model.AgsaReferralEmail;
            settings.EnforcementUnitEmail = model.EnforcementUnitEmail;

            // ---------------------------------------------------
            // Approval Workflow
            // ---------------------------------------------------
            settings.RequireManagerApproval = model.RequireManagerApproval;

            // ---------------------------------------------------
            // Synchronization
            // ---------------------------------------------------
            settings.SynchronizationIntervalHours = model.SynchronizationIntervalHours;

            // ---------------------------------------------------
            // Scheduled Reports
            // ---------------------------------------------------
            settings.SendMonthlyReportAutomatically = model.SendMonthlyReportAutomatically;
            settings.MonthlyReportRecipientEmail = model.MonthlyReportRecipientEmail;
            settings.MonthlyReportHourUtc = model.MonthlyReportHourUtc;
            // Do NOT touch settings.LastMonthlyReportSent - it's managed by the
            // MonthlyReportBackgroundService and must not be reset from the UI.

            // ---------------------------------------------------
            // Group Assignment
            // ---------------------------------------------------
            settings.AutoAssignmentEnabled = model.AutoAssignmentEnabled;
            settings.DistributionMethod = model.DistributionMethod;
            settings.DefaultMaximumOpenCases = model.DefaultMaximumOpenCases;
            // Do NOT touch settings.RoundRobinCursor - it's managed by RoundRobinStrategy.

            settings.DefaultPageSize = model.DefaultPageSize;

            await _repository.UpdateAsync(settings);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}