namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface ISynchronizationService
    {
        Task SynchronizeAsync(
        bool isManual,
        CancellationToken cancellationToken = default);
    }
}
