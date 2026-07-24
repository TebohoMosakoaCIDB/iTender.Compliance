using iTender.Compliance.Application.DTOs;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface ISigningHubService
    {
        Task AuthenticateAsync(CancellationToken cancellationToken = default);
        Task<SigningHubToken> ExchangeCodeAsync(
        string code,
        CancellationToken cancellationToken = default);
    }
}
