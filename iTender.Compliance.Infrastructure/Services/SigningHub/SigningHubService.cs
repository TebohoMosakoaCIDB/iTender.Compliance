using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Infrastructure.Models;
using Microsoft.Extensions.Options;

namespace iTender.Compliance.Infrastructure.Services.SigningHub
{
    public class SigningHubService : ISigningHubService
    {
        private readonly HttpClient _httpClient;
        private readonly SigningHubOptions _options;

        public SigningHubService(
            HttpClient httpClient,
            IOptions<SigningHubOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task AuthenticateAsync(
            CancellationToken cancellationToken = default)
        {

        }

        public Task<SigningHubToken> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public string GetAuthorizationUrl()
        {
            return
                $"{_options.BaseUrl}/oauth/authorize" +
                $"?response_type=code" +
                $"&client_id={_options.ClientId}" +
                $"&redirect_uri={Uri.EscapeDataString(_options.CallbackUrl)}";
        }
    }
}
