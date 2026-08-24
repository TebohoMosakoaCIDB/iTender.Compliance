using iTender.Application.DTOs;
using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces
{
    public interface IDataverseService
    {
        Task<List<Tender>> GetAdvertisedTendersAsync(
            CancellationToken cancellationToken = default);

        Task<List<ContractModel>> GetAwardedContractsAsync(
            DateTime fromDate,
            CancellationToken cancellationToken = default);
    }
}
