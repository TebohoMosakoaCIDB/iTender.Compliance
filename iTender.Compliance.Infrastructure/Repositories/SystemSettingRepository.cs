using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class SystemSettingRepository
    : RepositoryBase, ISystemSettingRepository
    {
        public SystemSettingRepository(
            ComplianceDbContext context)
            : base(context)
        {
        }

        public async Task<SystemSetting> GetAsync(
            CancellationToken cancellationToken = default)
        {
            var settings = await Context.SystemSettings
                .FirstOrDefaultAsync(cancellationToken);

            if (settings != null)
                return settings;

            settings = new SystemSetting();

            await Context.SystemSettings.AddAsync(
                settings,
                cancellationToken);

            await Context.SaveChangesAsync(cancellationToken);

            return settings;
        }

        public Task UpdateAsync(
            SystemSetting setting,
            CancellationToken cancellationToken = default)
        {
            Context.SystemSettings.Update(setting);

            return Task.CompletedTask;
        }
    }
}
