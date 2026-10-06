using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class SystemSettingRepository : RepositoryBase, ISystemSettingRepository
    {
        public SystemSettingRepository(IDbContextFactory<ComplianceDbContext> contextFactory)
            : base(contextFactory)
        {
        }

        public async Task<SystemSetting> GetAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var settings = await context.SystemSettings
                .FirstOrDefaultAsync(cancellationToken);

            if (settings != null)
                return settings;

            settings = new SystemSetting();

            await context.SystemSettings.AddAsync(settings, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return settings;
        }

        public async Task UpdateAsync(
            SystemSetting setting,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            context.SystemSettings.Update(setting);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}