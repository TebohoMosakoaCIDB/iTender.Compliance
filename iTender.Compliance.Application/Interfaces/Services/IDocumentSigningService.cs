namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IDocumentSigningService
    {
        Task<Guid> PrepareInstructionLetterForSigningAsync(
            Guid caseLetterId,
            CancellationToken cancellationToken = default);

        Task<Guid> PrepareReminderLetterForSigningAsync(
            Guid caseLetterId,
            CancellationToken cancellationToken = default);

        Task UploadPendingDocumentsAsync(
            CancellationToken cancellationToken = default);

        Task RefreshSigningStatusesAsync(
            CancellationToken cancellationToken = default);
    }
}
