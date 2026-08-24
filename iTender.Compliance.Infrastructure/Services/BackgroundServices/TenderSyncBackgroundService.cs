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


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait before first automatic run (as before)
            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var syncService = scope.ServiceProvider
                        .GetRequiredService<ISynchronizationService>();
                    var followUpService = scope.ServiceProvider
                        .GetRequiredService<IComplianceFollowUpService>();

                    // 1. Run tender synchronization
                    _logger.LogInformation("Starting scheduled tender synchronization");
                    await syncService.SynchronizeAsync(false, stoppingToken);
                    _logger.LogInformation("Scheduled tender synchronization completed");

                    // 2. Process overdue responses (follow‑up)
                    _logger.LogInformation("Starting overdue response processing");
                    await followUpService.ProcessOverdueResponsesAsync(stoppingToken);
                    _logger.LogInformation("Overdue response processing completed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Scheduled background task failed");
                }

                // Wait 12 hours before next run
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }
}
