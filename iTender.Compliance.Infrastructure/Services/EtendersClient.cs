using iTender.Compliance.Application.DTOs.Etenders;
using iTender.Compliance.Application.Filters;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Infrastructure.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace iTender.Compliance.Infrastructure.Services
{
    public class EtendersClient : IEtendersClient
    {
        private readonly HttpClient _httpClient;

        public EtendersClient(
            HttpClient httpClient,
            IOptions<EtendersOptions> options)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<EtendersRelease>> GetAllReleasesAsync(
    DateTime fromDate,
    DateTime toDate,
    int pageSize = 100,
    CancellationToken cancellationToken = default)
        {
            var allReleases = new List<EtendersRelease>();

            var currentPage = await GetReleasesAsync(
                fromDate,
                toDate,
                pageNumber: 1,
                pageSize: pageSize,
                cancellationToken);

            var constructionFilter = new EtendersConstructionFilter();

            var constructionTenders = constructionFilter.Filter(allReleases).ToList();

            constructionTenders.AddRange(currentPage.Releases);

            while (!string.IsNullOrWhiteSpace(currentPage.Links?.Next))
            {
                cancellationToken.ThrowIfCancellationRequested();

                currentPage = await GetPageAsync(
                    currentPage.Links.Next,
                    cancellationToken);

                constructionTenders.AddRange(currentPage.Releases);
            }

            return constructionTenders;
        }

        public async Task<EtendersResponse> GetReleasesAsync(
            DateTime fromDate,
            DateTime toDate,
            int pageNumber = 1,
            int pageSize = 100,
            CancellationToken cancellationToken = default)
        {
            var url =
                $"/api/OCDSReleases" +
                $"?PageNumber={pageNumber}" +
                $"&PageSize={pageSize}" +
                $"&dateFrom={fromDate:yyyy-MM-dd}" +
                $"&dateTo={toDate:yyyy-MM-dd}";

            var response = await _httpClient.GetAsync(
                url,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<EtendersResponse>(
                    cancellationToken);

            return result
                ?? throw new InvalidOperationException(
                    "eTenders API returned an empty response.");
        }

        private async Task<EtendersResponse> GetPageAsync(
            string url,
            CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(
                url,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<EtendersResponse>(
                    cancellationToken);

            return result
                ?? throw new InvalidOperationException(
                    "eTenders API returned an empty response.");
        }
    }
}
