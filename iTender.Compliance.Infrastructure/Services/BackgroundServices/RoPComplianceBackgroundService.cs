using iTender.Compliance.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace iTender.Compliance.Infrastructure.Services.BackgroundServices
{
    public class RoPComplianceBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RoPComplianceBackgroundService> _logger;

        public RoPComplianceBackgroundService(IServiceScopeFactory scopeFactory, ILogger<RoPComplianceBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IRoPComplianceService>();
                    await service.ProcessUnregisteredAwardsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "RoP compliance check failed");
                }

                // Run daily
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
