using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces
{
    public interface IDataverseService
    {
        Task<List<Tender>> GetAdvertisedTendersAsync(
            CancellationToken cancellationToken = default);
    }
}
