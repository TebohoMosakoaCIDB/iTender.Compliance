using iTender.Compliance.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace iTender.Compliance.Infrastructure.Services.BackgroundServices
{
    /// <summary>
    /// Drives the compliance workflow forward without manual intervention:
    /// sends reminder letters once the reminder delay has passed, then runs
    /// the escalation cycle (Instruction Letter -&gt; Contravention Notice -&gt;
    /// AGSA referral) for anything overdue. Runs on a loop rather than a
    /// fixed PeriodicTimer so the configured interval can change at runtime.
    /// </summary>
    public class ComplianceWorkflowBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ComplianceWorkflowBackgroundService> _logger;

        public ComplianceWorkflowBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ComplianceWorkflowBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Give the host a moment to finish starting up before the first pass.
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                var intervalMinutes = 60;

                using (var scope = _scopeFactory.CreateScope())
                {
                    var provider = scope.ServiceProvider;

                    try
                    {
                        var settingsService = provider.GetRequiredService<ISystemSettingService>();
                        var settings = await settingsService.GetAsync();

                        intervalMinutes = settings.ReminderCheckIntervalMinutes > 0
                            ? settings.ReminderCheckIntervalMinutes
                            : 60;

                        var reminderService = provider.GetRequiredService<IReminderService>();
                        await reminderService.ProcessRemindersAsync(stoppingToken);

                        var escalationService = provider.GetRequiredService<IEscalationService>();
                        await escalationService.RunEscalationCycleAsync(stoppingToken);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(
                            ex,
                            "Compliance workflow background pass failed.");
                    }
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}