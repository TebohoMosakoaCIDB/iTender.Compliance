namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface ICaseAuthorizationService
    {
        Task<bool> CanViewAsync(
            Guid caseId,
            CancellationToken cancellationToken = default);

        Task<bool> CanEditAsync(
            Guid caseId,
            CancellationToken cancellationToken = default);

        Task<bool> CanAssignAsync(
            Guid caseId,
            CancellationToken cancellationToken = default);

        Task<bool> CanCloseAsync(
            Guid caseId,
            CancellationToken cancellationToken = default);

        Task<bool> CanViewAllCasesAsync();

        Task<Guid?> GetCurrentAgentIdAsync();
    }
}
