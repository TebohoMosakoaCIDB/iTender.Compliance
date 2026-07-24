using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Scrapers
{
    public interface IScraperService
    {
        string Name { get; }

        Task<List<Tender>> ScrapeAsync(
            CancellationToken cancellationToken = default);
    }
}
