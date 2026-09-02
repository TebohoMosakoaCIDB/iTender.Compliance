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
                ResponseDueHours = settings.ResponseDueHours,
                ReminderDelayHours = settings.ReminderDelayHours,
                SynchronizationIntervalHours = settings.SynchronizationIntervalHours,
                MaximumReminders = settings.MaximumReminders,
                DefaultPageSize = settings.DefaultPageSize,
                DistributionMethod = settings.DistributionMethod,
                ClosedTenderResponseDays = settings.ClosedTenderResponseDays,
                ContraventionNoticeResponseDays = settings.ContraventionNoticeResponseDays,
                InstructionLetterResponseWorkingDays = settings.InstructionLetterResponseWorkingDays,
                ReminderAfterWorkingDays = settings.ReminderAfterWorkingDays,
                AgsaReferralDeadlineDays = settings.AgsaReferralDeadlineDays,
                RopCheckAfterDays = settings.RopCheckAfterDays,
                RopRegistrationGraceDays = settings.RopRegistrationGraceDays,
                AgsaReferralEmail = settings.AgsaReferralEmail,
                EnforcementUnitEmail = settings.EnforcementUnitEmail,
                RequireManagerApproval = settings.RequireManagerApproval
            };
        }

        public async Task SaveAsync(SystemSettingModel model)
        {
            var settings = await _repository.GetAsync();

            settings.ResponseDueHours = model.ResponseDueHours;
            settings.ReminderDelayHours = model.ReminderDelayHours;
            settings.SynchronizationIntervalHours = model.SynchronizationIntervalHours;
            settings.MaximumReminders = model.MaximumReminders;
            settings.DefaultPageSize = model.DefaultPageSize;
            settings.AutoAssignmentEnabled = model.AutoAssignmentEnabled;
            settings.DistributionMethod = model.DistributionMethod;
            settings.ClosedTenderResponseDays = model.ClosedTenderResponseDays;
            settings.ContraventionNoticeResponseDays = model.ContraventionNoticeResponseDays;
            settings.InstructionLetterResponseWorkingDays = model.InstructionLetterResponseWorkingDays;
            settings.ReminderAfterWorkingDays = model.ReminderAfterWorkingDays;
            settings.AgsaReferralDeadlineDays = model.AgsaReferralDeadlineDays;
            settings.RopCheckAfterDays = model.RopCheckAfterDays;
            settings.RopRegistrationGraceDays = model.RopRegistrationGraceDays;
            settings.AgsaReferralEmail = model.AgsaReferralEmail;
            settings.EnforcementUnitEmail = model.EnforcementUnitEmail;
            settings.RequireManagerApproval = model.RequireManagerApproval;

            await _repository.UpdateAsync(settings);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}