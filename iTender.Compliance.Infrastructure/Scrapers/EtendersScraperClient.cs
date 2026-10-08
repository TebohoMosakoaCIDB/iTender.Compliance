using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Scrapers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace iTender.Compliance.Infrastructure.Scrapers
{
    public class EtendersScraperClient : IEtendersScraperClient
    {
        private readonly HttpClient _http;
        private readonly ScraperOptions _options;
        private readonly ILogger<EtendersScraperClient> _logger;

        public EtendersScraperClient(
            HttpClient http,
            IOptions<ScraperOptions> options,
            ILogger<EtendersScraperClient> logger)
        {
            _http = http;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<ScrapeResponse?> ScrapeAsync(
            int status = 1,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("Scraper disabled in configuration.");
                return null;
            }

            try
            {
                _logger.LogInformation("Calling scraper service.");

                var response = await _http.PostAsync(
                    $"/scrape/etenders?status={status}",
                    content: null,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var result = await response.Content
                    .ReadFromJsonAsync<ScrapeResponse>(cancellationToken);

                _logger.LogInformation(
                    "Scraper returned {Count} tenders.",
                    result?.Count ?? 0);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scraper service call failed.");
                return null;
            }
        }
    }
}
