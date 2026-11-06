namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface ISynchronizationService
    {
        Task SynchronizeAsync(
            bool isManual,
            CancellationToken cancellationToken = default);

        Task ProcessClassifiedTenderAsync(
            Guid tenderId,
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
