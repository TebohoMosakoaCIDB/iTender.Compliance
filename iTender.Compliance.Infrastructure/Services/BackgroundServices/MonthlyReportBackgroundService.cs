using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace iTender.Compliance.Infrastructure.Services.BackgroundServices
{
    /// <summary>
    /// BR-9.4: emails the previous month's compliance report on the 1st of each month
    /// and the previous quarter's report on the 1st of Jan, Apr, Jul and Oct.
    /// Runs hourly and checks the configured send hour.
    /// </summary>
    public class MonthlyReportBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MonthlyReportBackgroundService> _logger;

        public MonthlyReportBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<MonthlyReportBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Give the host time to start before the first check.
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;

                using (var scope = _scopeFactory.CreateScope())
                {
                    try
                    {
                        await TrySendMonthlyReportAsync(
                            scope.ServiceProvider, now, stoppingToken);

                        await TrySendQuarterlyReportAsync(
                            scope.ServiceProvider, now, stoppingToken);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(
                            ex,
                            "Scheduled reports background pass failed.");
                    }
                }

                try
                {
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        // ---------------------------------------------------------------------
        // MONTHLY
        // ---------------------------------------------------------------------

        private async Task TrySendMonthlyReportAsync(
            IServiceProvider services,
            DateTime now,
            CancellationToken cancellationToken)
        {
            // Only attempt on the 1st of the month.
            if (now.Day != 1)
                return;

            var settingsService = services.GetRequiredService<ISystemSettingService>();
            var settings = await settingsService.GetAsync();

            if (!settings.SendMonthlyReportAutomatically)
                return;

            if (string.IsNullOrWhiteSpace(settings.MonthlyReportRecipientEmail))
            {
                _logger.LogWarning(
                    "Monthly report is enabled but no recipient is configured. Skipping.");
                return;
            }

            // Only run at the configured hour.
            if (now.Hour != settings.MonthlyReportHourUtc)
                return;

            // Previous month.
            var prev = now.AddMonths(-1);
            var marker = $"{prev.Year:D4}-{prev.Month:D2}";

            // Already sent for this month?
            if (string.Equals(settings.LastMonthlyReportSent, marker, StringComparison.Ordinal))
                return;

            _logger.LogInformation(
                "Generating scheduled monthly report for {Marker}.", marker);

            var reportingService = services.GetRequiredService<IReportingService>();
            var pdfService = services.GetRequiredService<IReportPdfService>();
            var emailService = services.GetRequiredService<IEmailService>();
            var auditService = services.GetRequiredService<IAuditService>();
            var settingRepo = services.GetRequiredService<ISystemSettingRepository>();
            var unitOfWork = services.GetRequiredService<IUnitOfWork>();

            var report = await reportingService
                .GetMonthlyReportAsync(prev.Year, prev.Month, cancellationToken);

            var pdfBytes = await pdfService.GenerateAsync(
                report.PeriodStart,
                report.PeriodEnd);

            var tempDir = Path.Combine(Path.GetTempPath(), "crcip-reports");
            Directory.CreateDirectory(tempDir);

            var fileName = $"CRCIP_Monthly_{prev.Year:D4}{prev.Month:D2}.pdf";
            var tempPath = Path.Combine(tempDir, fileName);

            await File.WriteAllBytesAsync(tempPath, pdfBytes, cancellationToken);

            try
            {
                await emailService.SendAsync(new EmailMessageModel
                {
                    ToAddress = settings.MonthlyReportRecipientEmail!,
                    Subject = $"CRCIP Monthly Compliance Report — {report.PeriodLabel}",
                    Body =
                        $"<p>Please find attached the compliance report for " +
                        $"<strong>{report.PeriodLabel}</strong>.</p>" +
                        "<p>Regards,<br/>iTender Compliance</p>",
                    AttachmentPaths = new List<string> { tempPath }
                }, cancellationToken);
            }
            finally
            {
                try
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }
                catch
                {
                    // Best-effort cleanup - don't fail the send if delete fails.
                }
            }

            // Mark as sent.
            var freshSettings = await settingRepo.GetAsync(cancellationToken);
            freshSettings.LastMonthlyReportSent = marker;
            await settingRepo.UpdateAsync(freshSettings, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await auditService.LogAsync(
                Domain.Enums.AuditAction.SyncCompleted,
                Domain.Enums.AuditEntity.TenderSync,
                Guid.Empty,
                $"Monthly compliance report for {report.PeriodLabel} " +
                $"sent to {settings.MonthlyReportRecipientEmail}.",
                null,
                cancellationToken);

            _logger.LogInformation(
                "Monthly report {Marker} sent to {Recipient}.",
                marker, settings.MonthlyReportRecipientEmail);
        }

        // ---------------------------------------------------------------------
        // QUARTERLY
        // ---------------------------------------------------------------------

        private async Task TrySendQuarterlyReportAsync(
            IServiceProvider services,
            DateTime now,
            CancellationToken cancellationToken)
        {
            // Only on the 1st of Jan, Apr, Jul, Oct.
            if (now.Day != 1 ||
                (now.Month != 1 && now.Month != 4 && now.Month != 7 && now.Month != 10))
                return;

            var settingsService = services.GetRequiredService<ISystemSettingService>();
            var settings = await settingsService.GetAsync();

            if (!settings.SendQuarterlyReportAutomatically)
                return;

            if (string.IsNullOrWhiteSpace(settings.QuarterlyReportRecipientEmail))
            {
                _logger.LogWarning(
                    "Quarterly report is enabled but no recipient is configured. Skipping.");
                return;
            }

            // Only run at the configured hour.
            if (now.Hour != settings.MonthlyReportHourUtc)
                return;

            // Previous quarter.
            var prevMonth = now.AddMonths(-1);
            var quarter = ((prevMonth.Month - 1) / 3) + 1;
            var marker = $"{prevMonth.Year:D4}-Q{quarter}";

            // Already sent for this quarter?
            if (string.Equals(settings.LastQuarterlyReportSent, marker, StringComparison.Ordinal))
                return;

            _logger.LogInformation(
                "Generating scheduled quarterly report for {Marker}.", marker);

            var reportingService = services.GetRequiredService<IReportingService>();
            var pdfService = services.GetRequiredService<IReportPdfService>();
            var emailService = services.GetRequiredService<IEmailService>();
            var auditService = services.GetRequiredService<IAuditService>();
            var settingRepo = services.GetRequiredService<ISystemSettingRepository>();
            var unitOfWork = services.GetRequiredService<IUnitOfWork>();

            var report = await reportingService
                .GetQuarterlyReportAsync(prevMonth.Year, quarter, cancellationToken);

            var pdfBytes = await pdfService.GenerateAsync(
                report.PeriodStart,
                report.PeriodEnd);

            var tempDir = Path.Combine(Path.GetTempPath(), "crcip-reports");
            Directory.CreateDirectory(tempDir);

            var fileName = $"CRCIP_Quarterly_{prevMonth.Year:D4}Q{quarter}.pdf";
            var tempPath = Path.Combine(tempDir, fileName);

            await File.WriteAllBytesAsync(tempPath, pdfBytes, cancellationToken);

            try
            {
                await emailService.SendAsync(new EmailMessageModel
                {
                    ToAddress = settings.QuarterlyReportRecipientEmail!,
                    Subject = $"CRCIP Quarterly Compliance Report — {report.PeriodLabel}",
                    Body =
                        $"<p>Please find attached the quarterly compliance report for " +
                        $"<strong>{report.PeriodLabel}</strong>.</p>" +
                        "<p>Regards,<br/>iTender Compliance</p>",
                    AttachmentPaths = new List<string> { tempPath }
                }, cancellationToken);
            }
            finally
            {
                try
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }
                catch
                {
                    // Best-effort cleanup.
                }
            }

            // Mark as sent.
            var freshSettings = await settingRepo.GetAsync(cancellationToken);
            freshSettings.LastQuarterlyReportSent = marker;
            await settingRepo.UpdateAsync(freshSettings, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await auditService.LogAsync(
                Domain.Enums.AuditAction.SyncCompleted,
                Domain.Enums.AuditEntity.TenderSync,
                Guid.Empty,
                $"Quarterly compliance report for {report.PeriodLabel} " +
                $"sent to {settings.QuarterlyReportRecipientEmail}.",
                null,
                cancellationToken);

            _logger.LogInformation(
                "Quarterly report {Marker} sent to {Recipient}.",
                marker, settings.QuarterlyReportRecipientEmail);
        }
    }
}