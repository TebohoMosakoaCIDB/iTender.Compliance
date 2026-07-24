namespace iTender.Compliance.Application.Interfaces.Services;

public interface IPdfReportService
{
    Task<byte[]> GenerateComplianceSummaryAsync(
        CancellationToken cancellationToken = default);

    Task<byte[]> GenerateOutstandingCasesAsync(
        CancellationToken cancellationToken = default);

    Task<byte[]> GenerateAgentPerformanceAsync(
        CancellationToken cancellationToken = default);

    Task<byte[]> GenerateSynchronizationHistoryAsync(
        CancellationToken cancellationToken = default);

    Task<byte[]> GenerateLetterHistoryAsync(
        CancellationToken cancellationToken = default);

    Task<byte[]> GenerateAuditHistoryAsync(
        CancellationToken cancellationToken = default);
}