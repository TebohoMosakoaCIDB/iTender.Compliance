using iTender.Compliance.Application.DTOs.Etenders;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IEtendersClient
    {
        Task<EtendersResponse> GetReleasesAsync(
            DateTime fromDate,
            DateTime toDate,
            int pageNumber = 1,
            int pageSize = 100,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<EtendersRelease>> GetAllReleasesAsync(
            DateTime fromDate,
            DateTime toDate,
            int pageSize = 100,
            CancellationToken cancellationToken = default);
    }
}
