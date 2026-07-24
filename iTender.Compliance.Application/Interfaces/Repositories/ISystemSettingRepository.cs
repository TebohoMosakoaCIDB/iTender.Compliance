using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface ISystemSettingRepository
    {
        Task<SystemSetting> GetAsync(
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            SystemSetting setting,
            CancellationToken cancellationToken = default);
    }
}
