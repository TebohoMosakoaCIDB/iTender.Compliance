namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IClientResponseService
    {
        Task<Guid> SubmitAsync(
            Guid complianceCaseId,
            Guid caseLetterId,
            string? clientComments,
            CancellationToken cancellationToken = default);

        Task BeginReviewAsync(
            Guid responseId,
            CancellationToken cancellationToken = default);

        Task RequestAdditionalInformationAsync(
            Guid responseId,
            string comments,
            DateTime? dueOn = null,
            CancellationToken cancellationToken = default);

        Task MarkIncompleteAsync(
            Guid responseId,
            string comments,
            CancellationToken cancellationToken = default);

        Task AcceptAsync(
            Guid responseId,
            string? comments,
            CancellationToken cancellationToken = default);

        Task RejectAsync(
            Guid responseId,
            string comments,
            CancellationToken cancellationToken = default);

        Task<Guid> ResubmitAsync(
            Guid previousResponseId,
            string? clientComments,
            CancellationToken cancellationToken = default);
    }
}