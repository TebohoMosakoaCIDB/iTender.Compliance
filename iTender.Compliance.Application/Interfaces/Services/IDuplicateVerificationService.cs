namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IDuplicateVerificationService
    {
        Task ConfirmDuplicateAsync(Guid groupId, Guid coUserId, string? comment, CancellationToken cancellationToken = default);
        Task RejectDuplicateAsync(Guid groupId, Guid coUserId, string? comment, CancellationToken cancellationToken = default);
    }
}
