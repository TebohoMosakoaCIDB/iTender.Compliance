using iTender.Compliance.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace iTender.Compliance.Infrastructure.Services.BackgroundServices
{
    public class AutoAssignmentBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public AutoAssignmentBackgroundService(
            IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            using var timer =
                new PeriodicTimer(TimeSpan.FromHours(6));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                using var scope = _scopeFactory.CreateScope();

                var service = scope.ServiceProvider
                    .GetRequiredService<IAutoAssignmentService>();

                await service.AssignUnassignedCasesAsync(stoppingToken);
            }
        }
    }
}
