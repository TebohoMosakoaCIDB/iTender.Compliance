using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Application.Providers;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace iTender.Compliance.Infrastructure.Providers
{
    public class SigningHubApiProvider : ISigningHubApiProvider
    {
        private readonly HttpClient _httpClient;
        private readonly SigningHubOptions _options;

        public SigningHubApiProvider(
            HttpClient httpClient,
            IOptions<SigningHubOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        /// <summary>SigningHub returns a JSON error body describing exactly what was wrong with the
        /// request (invalid field, unsupported value for this tenant, etc.) - EnsureSuccessStatusCode()
        /// alone discards that, leaving only "400 Bad Request" to debug from. Always read and surface it.</summary>
        private static async Task EnsureSuccessAsync(HttpResponseMessage response, string operation)
        {
            if (response.IsSuccessStatusCode)
                return;

            var body = await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                $"SigningHub {operation} failed with {(int)response.StatusCode} {response.StatusCode}: {body}");
        }

        public async Task<SigningHubTokenResponse> AuthenticateAsync()
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "authenticate");

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "client_id", _options.ClientId },
                    { "client_secret", _options.ClientSecret },
                    { "username", _options.Username },
                    { "password", _options.Password }
                });

            var response = await _httpClient.SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(body);

            return JsonSerializer.Deserialize<SigningHubTokenResponse>(
                body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
        }

        public async Task AddSignerAsync(string accessToken, int packageId, string email, string name, int signingOrder = 1)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"v4/packages/{packageId}/workflow/users");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            request.Content = JsonContent.Create(new[]
            {
                new AddSignerRequest
                {
                    UserEmail = email,
                    UserName = name,
                    SigningOrder = signingOrder
                }
            });

            var response = await _httpClient.SendAsync(request);

            await EnsureSuccessAsync(response, "add signer");
        }

        public async Task<SigningStatusModel> GetStatusAsync(string accessToken, int packageId)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"v4/packages/{packageId}/log");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);

            await EnsureSuccessAsync(response, "get status");

            var result = await response.Content.ReadFromJsonAsync<SigningStatusModel>();

            if (result == null)
                throw new InvalidOperationException("Unable to retrieve SigningHub package status.");

            return result;
        }

        public async Task<byte[]> DownloadSignedDocumentAsync(string accessToken, int packageId)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"v4/packages/{packageId}/base64");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            var response = await _httpClient.SendAsync(request);

            await EnsureSuccessAsync(response, "download signed document");

            var result = await response.Content
                .ReadFromJsonAsync<DownloadDocumentResponse>();

            if (result == null || string.IsNullOrWhiteSpace(result.Base64))
                throw new InvalidOperationException("Signed document was not returned.");

            return Convert.FromBase64String(result.Base64);
        }

        public async Task<int> UploadDocumentAsync(string accessToken, int packageId, string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("The document could not be found.", filePath);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"v4/packages/{packageId}/documents");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            request.Headers.Add("x-file-name", Path.GetFileName(filePath));
            request.Headers.Add("x-convert-document", "true");
            request.Headers.Add("x-source", "API");

            var bytes = await File.ReadAllBytesAsync(filePath);

            request.Content = new ByteArrayContent(bytes);
            request.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            var response = await _httpClient.SendAsync(request);

            await EnsureSuccessAsync(response, "upload document");

            var result = await response.Content.ReadFromJsonAsync<UploadDocumentResponse>();

            if (result == null || result.DocumentId <= 0)
                throw new InvalidOperationException("SigningHub did not return a valid document id.");

            return result.DocumentId;
        }

        public async Task<int> CreatePackageAsync(string accessToken, string packageName)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "v4/packages");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            request.Content = JsonContent.Create(new CreatePackageRequest
            {
                PackageName = packageName
            });

            var response = await _httpClient.SendAsync(request);

            await EnsureSuccessAsync(response, "create package");

            var result = await response.Content
                .ReadFromJsonAsync<CreatePackageResponse>();

            if (result == null || result.PackageId <= 0)
                throw new InvalidOperationException("Failed to create SigningHub package.");

            return result.PackageId;
        }

        public async Task PlaceSignatureFieldAsync(string accessToken, int packageId, int documentId)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"v4/packages/{packageId}/documents/{documentId}/fields/autoplace");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            request.Content = JsonContent.Create(new AutoplaceSignatureRequest());

            var response = await _httpClient.SendAsync(request);

            await EnsureSuccessAsync(response, "place signature field");
        }

        public async Task SharePackageAsync(string accessToken, int packageId)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"v4/packages/{packageId}/workflow");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            request.Content = JsonContent.Create(new { });

            var response = await _httpClient.SendAsync(request);

            await EnsureSuccessAsync(response, "share package");
        }

        public Task<string> GenerateIntegrationLinkAsync(string accessToken, int packageId, string email)
        {
            throw new NotImplementedException();
        }
    }
}