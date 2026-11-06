namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IClassOfWorkReviewService
    {
        Task ResolveAsync(
            Guid findingId,
            string selectedClassOfWorks,
            string? notes,
            Guid officerUserId,
            CancellationToken cancellationToken = default);

        Task DeferAsync(
            Guid findingId,
            string reason,
            Guid officerUserId,
            CancellationToken cancellationToken = default);
    }
}
