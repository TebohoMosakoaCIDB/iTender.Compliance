using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Providers
{
    public interface ISigningHubApiProvider
    {
        Task<SigningHubTokenResponse> AuthenticateAsync();

        Task<int> CreatePackageAsync(
            string accessToken,
            string packageName);

        Task<int> UploadDocumentAsync(
            string accessToken,
            int packageId,
            string filePath);

        Task AddSignerAsync(
            string accessToken,
            int packageId,
            string email,
            string name,
            int signingOrder = 1);

        Task PlaceSignatureFieldAsync(
            string accessToken,
            int packageId,
            int documentId);

        Task SharePackageAsync(
            string accessToken,
            int packageId);

        Task<SigningStatusModel> GetStatusAsync(
            string accessToken,
            int packageId);

        Task<byte[]> DownloadSignedDocumentAsync(
            string accessToken,
            int packageId);
    }
}
