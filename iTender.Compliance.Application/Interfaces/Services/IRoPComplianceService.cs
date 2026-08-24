namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IRoPComplianceService
    {
        Task ProcessUnregisteredAwardsAsync(CancellationToken cancellationToken = default);
    }
}
