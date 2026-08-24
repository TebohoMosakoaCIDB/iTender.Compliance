using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Scrapers;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace iTender.Compliance.Infrastructure.Scrapers
{
    public class ETenderApiScraper : IScraperService
    {
        private readonly HttpClient _httpClient;
        private readonly ICategoryMappingService _categoryMappingService;
        private readonly ILogger<ETenderApiScraper> _logger;

        public ETenderApiScraper(
            HttpClient httpClient,
            IOptions<ETenderApiOptions> options,
            ICategoryMappingService categoryMappingService,
            ILogger<ETenderApiScraper> logger)
        {
            _httpClient = httpClient;
            _categoryMappingService = categoryMappingService;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);

            if (!string.IsNullOrEmpty(options.Value.Username) && !string.IsNullOrEmpty(options.Value.Password))
            {
                var byteArray = Encoding.ASCII.GetBytes($"{options.Value.Username}:{options.Value.Password}");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
        }

        public string Name => "EtenderApi";

        public async Task<List<Tender>> ScrapeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("Tenders", cancellationToken);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var apiTenders = JsonSerializer.Deserialize<List<ETenderApiTender>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiTenders == null)
                    return new List<Tender>();

                var tenders = new List<Tender>();
                foreach (var apiTender in apiTenders)
                {
                    var (isConstruction, classOfWork) = await _categoryMappingService.MapCategoryAsync(apiTender.Category, cancellationToken);

                    // Build Tender entity
                    var tender = new Tender
                    {
                        TenderNumber = apiTender.TenderNumber,
                        Title = apiTender.Description ?? apiTender.TenderNumber,
                        Description = apiTender.Description,
                        EmployerName = apiTender.Department,
                        ContactName = apiTender.ContactPerson,
                        ContactEmail = apiTender.Email,
                        ContactNumber = apiTender.Telephone,

                        AdvertisedDate = apiTender.DatePublished.HasValue
                        ? DateTime.SpecifyKind(apiTender.DatePublished.Value, DateTimeKind.Utc)
                        : DateTime.UtcNow,

                                        ClosingDate = apiTender.ClosingDate.HasValue
                        ? DateTime.SpecifyKind(apiTender.ClosingDate.Value, DateTimeKind.Utc)
                        : DateTime.UtcNow.AddDays(30),

                        Source = "ETender API",
                        TenderUrl = apiTender.Url ?? string.Empty,
                        IsConstruction = isConstruction,
                        ClassOfWorks = classOfWork
                    };

                    tenders.Add(tender);
                }

                _logger.LogInformation("ETender API returned {Count} tenders.", tenders.Count);
                return tenders;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "ETender API request failed.");
                return new List<Tender>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse ETender API response.");
                return new List<Tender>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while scraping ETender API.");
                return new List<Tender>();
            }
        }
    }

    public class ETenderApiOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

}
