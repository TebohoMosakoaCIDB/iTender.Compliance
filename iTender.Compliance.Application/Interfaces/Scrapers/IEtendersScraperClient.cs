using iTender.Compliance.Application.DTOs;

namespace iTender.Compliance.Application.Interfaces.Scrapers
{
    public interface IEtendersScraperClient
    {
        Task<ScrapeResponse?> ScrapeAsync(
            int status = 1,
            CancellationToken cancellationToken = default);
    }
}
