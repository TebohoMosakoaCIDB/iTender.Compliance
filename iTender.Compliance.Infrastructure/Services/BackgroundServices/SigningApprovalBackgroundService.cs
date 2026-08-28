using iTender.Compliance.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace iTender.Compliance.Infrastructure.Services.BackgroundServices
{
    /// <summary>
    /// Polls SigningHub for outstanding manager-approval requests. Deliberately
    /// separate from <see cref="ComplianceWorkflowBackgroundService"/> so signing
    /// status - which a Manager may action within minutes - can be checked far
    /// more often than the hourly reminder/escalation cycle, without hammering
    /// SigningHub on every reminder pass.
    /// </summary>
    public class SigningApprovalBackgroundService : BackgroundService
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(5);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SigningApprovalBackgroundService> _logger;

        public SigningApprovalBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<SigningApprovalBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(45), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var provider = scope.ServiceProvider;

                    try
                    {
                        var signingService = provider.GetRequiredService<IDocumentSigningService>();
                        var result = await signingService.PollAndCompleteAsync(stoppingToken);

                        if (result.CompletedCaseLetterIds.Count > 0 ||
                            result.RejectedLetters.Count > 0)
                        {
                            var correspondenceService = provider.GetRequiredService<ICorrespondenceService>();

                            foreach (var caseLetterId in result.CompletedCaseLetterIds)
                            {
                                await correspondenceService.CompleteApprovedLetterAsync(
                                    caseLetterId,
                                    stoppingToken);
                            }

                            foreach (var rejected in result.RejectedLetters)
                            {
                                await correspondenceService.HandleRejectedLetterAsync(
                                    rejected.CaseLetterId,
                                    rejected.Reason,
                                    stoppingToken);
                            }
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "SigningHub approval polling pass failed.");
                    }
                }

                try
                {
                    await Task.Delay(PollInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}