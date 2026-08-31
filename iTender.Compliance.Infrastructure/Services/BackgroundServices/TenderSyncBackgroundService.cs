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

                    // 1. Run tender synchronization
                    _logger.LogInformation("Starting scheduled tender synchronization");
                    await syncService.SynchronizeAsync(false, stoppingToken);
                    _logger.LogInformation("Scheduled tender synchronization completed");

                    // NOTE: overdue-response follow-up (IL -> CN -> AGSA) is handled by
                    // EscalationService via ComplianceWorkflowBackgroundService instead of
                    // IComplianceFollowUpService here. That path actually generates and emails
                    // the Contravention Notice/AGSA referral and respects the SigningHub
                    // approval gate; this one only wrote bare DB records with no document or
                    // delivery, and its CaseStatus check for the AGSA branch never matched
                    // (both branches tested WaitingForResponse), so it would have kept
                    // re-issuing empty Contravention Notices instead of ever escalating.
                    // Fix that bug (or retire IComplianceFollowUpService) before re-enabling.
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