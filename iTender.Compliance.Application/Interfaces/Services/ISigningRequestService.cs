using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface ISigningRequestService
    {
        Task<Guid> CreateAsync(
            Guid caseLetterId,
            Guid signerId,
            string signerName,
            string signerEmail,
            CancellationToken cancellationToken = default);

        Task<SigningRequest?> GetAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task MarkUploadedAsync(
            Guid signingRequestId,
            string workflowId,
            string documentId,
            CancellationToken cancellationToken = default);

        Task MarkSignedAsync(
            Guid signingRequestId,
            string signedDocumentPath,
            CancellationToken cancellationToken = default);

        Task MarkFailedAsync(
            Guid signingRequestId,
            string reason,
            CancellationToken cancellationToken = default);
    }
}
