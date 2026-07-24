using iTender.Compliance.Application.DTOs;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface ISystemSettingService
    {
        Task<SystemSettingModel> GetAsync();

        Task SaveAsync(SystemSettingModel model);
    }
}
