using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace iTender.Compliance.Infrastructure.Data
{
    public static class SystemSettingSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider
                .GetRequiredService<ComplianceDbContext>();

            var settings = await context.SystemSettings
                .OrderBy(x => x.CreatedOn)
                .FirstOrDefaultAsync();

            if (settings == null)
            {
                context.SystemSettings.Add(new SystemSetting
                {
                    Id = Guid.NewGuid(),
                    AutoAssignmentEnabled = true,
                    DistributionMethod = CaseDistributionMethod.RoundRobin,
                    RoundRobinCursor = 0,
                    DefaultMaximumOpenCases = 10,
                    CreatedOn = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
                return;
            }

            var changed = false;

            if (!settings.AutoAssignmentEnabled)
            {
                settings.AutoAssignmentEnabled = true;
                changed = true;
            }

            if (settings.DistributionMethod != CaseDistributionMethod.RoundRobin)
            {
                settings.DistributionMethod = CaseDistributionMethod.RoundRobin;
                changed = true;
            }

            if (changed)
                await context.SaveChangesAsync();
        }
    }
}