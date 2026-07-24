using iTender.Compliance.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace iTender.Compliance.Infrastructure.Services.BackgroundServices
{
    public class TenderSyncBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TenderSyncBackgroundService> _logger;

        public TenderSyncBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<TenderSyncBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            // Wait before first automatic run
            await Task.Delay(
                TimeSpan.FromHours(12),
                stoppingToken);


            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var syncService = scope.ServiceProvider
                        .GetRequiredService<ISynchronizationService>();

                    _logger.LogInformation(
                        "Starting scheduled tender synchronization");


                    await syncService.SynchronizeAsync(
                        false,
                        stoppingToken);


                    _logger.LogInformation(
                        "Scheduled tender synchronization completed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Scheduled tender synchronization failed");
                }


                // Every 12 hours
                await Task.Delay(
                    TimeSpan.FromHours(12),
                    stoppingToken);
            }
        }
    }
}
