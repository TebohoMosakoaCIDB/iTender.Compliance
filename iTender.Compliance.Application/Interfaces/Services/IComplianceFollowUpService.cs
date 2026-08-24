namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IComplianceFollowUpService
    {
        Task ProcessOverdueResponsesAsync(CancellationToken cancellationToken = default);
        Task ProcessExtensionRequestAsync(Guid caseId, int extensionHours, CancellationToken cancellationToken = default);
        Task RecordResponseAsync(Guid letterId, bool accepted, string? comments, CancellationToken cancellationToken = default);
        Task EscalateToAGSAAsync(Guid caseId, CancellationToken cancellationToken = default);
    }
}
