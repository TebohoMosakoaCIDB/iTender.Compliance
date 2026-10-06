namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface ITenderClassificationService
    {
        Task ConfirmAsync(
            Guid tenderId,
            Guid userId,
            string? notes,
            CancellationToken cancellationToken = default);

        Task<int> ConfirmManyAsync(
            IEnumerable<Guid> tenderIds,
            Guid userId,
            string? notes,
            CancellationToken cancellationToken = default);

        Task OverrideAsync(
            Guid tenderId,
            bool isConstruction,
            Guid userId,
            string reason,
            CancellationToken cancellationToken = default);
    }
}