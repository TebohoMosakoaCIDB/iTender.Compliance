using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Application.Providers;
using iTender.Compliance.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace iTender.Compliance.Infrastructure.Services.SigningHub
{
    public class DocumentSigningService : IDocumentSigningService
    {
        private readonly ISigningHubApiProvider _signingHubApi;
        private readonly ISigningRequestService _signingRequestService;
        private readonly ISigningRequestRepository _signingRequestRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DocumentSigningService> _logger;

        public DocumentSigningService(
            ISigningHubApiProvider signingHubApi,
            ISigningRequestService signingRequestService,
            ISigningRequestRepository signingRequestRepository,
            IConfiguration configuration,
            ILogger<DocumentSigningService> logger)
        {
            _signingHubApi = signingHubApi;
            _signingRequestService = signingRequestService;
            _signingRequestRepository = signingRequestRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task RequestApprovalAsync(
            CaseLetter letter,
            Agent manager,
            CancellationToken cancellationToken = default)
        {
            var requestId = await _signingRequestService.CreateAsync(
                letter.Id,
                manager.Id,
                manager.FullName,
                manager.Email,
                cancellationToken);

            try
            {
                var token = await _signingHubApi.AuthenticateAsync();

                var packageId = await _signingHubApi.CreatePackageAsync(
                    token.AccessToken,
                    $"{letter.Type} - {letter.RecipientName} - {letter.FileName}");

                var documentId = await _signingHubApi.UploadDocumentAsync(
                    token.AccessToken,
                    packageId,
                    letter.FilePath);

                await _signingHubApi.AddSignerAsync(
                    token.AccessToken,
                    packageId,
                    manager.Email,
                    manager.FullName);

                await _signingHubApi.PlaceSignatureFieldAsync(
                    token.AccessToken,
                    packageId,
                    documentId);

                await _signingHubApi.SharePackageAsync(
                    token.AccessToken,
                    packageId);

                var request = await _signingRequestRepository.GetByIdAsync(
                    requestId,
                    cancellationToken);

                if (request != null)
                {
                    request.PackageId = packageId;

                    await _signingRequestRepository.UpdateAsync(
                        request,
                        cancellationToken);
                }

                await _signingRequestService.MarkUploadedAsync(
                    requestId,
                    packageId.ToString(),
                    documentId.ToString(),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to push case letter {CaseLetterId} to SigningHub for approval.",
                    letter.Id);

                await _signingRequestService.MarkFailedAsync(
                    requestId,
                    ex.Message,
                    cancellationToken);

                throw;
            }
        }

        public async Task<SigningPollResult> PollAndCompleteAsync(
            CancellationToken cancellationToken = default)
        {
            var result = new SigningPollResult();

            var pending = await _signingRequestRepository.GetPendingAsync(cancellationToken);

            // Only requests that have actually been pushed to SigningHub have a package to check.
            var toCheck = pending
                .Where(x => x.PackageId.HasValue)
                .ToList();

            if (toCheck.Count == 0)
                return result;

            string accessToken;

            try
            {
                var token = await _signingHubApi.AuthenticateAsync();
                accessToken = token.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to authenticate with SigningHub while polling status.");
                return result;
            }

            foreach (var request in toCheck)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var status = await _signingHubApi.GetStatusAsync(
                        accessToken,
                        request.PackageId!.Value);

                    if (status.IsCompleted)
                    {
                        var signedBytes = await _signingHubApi.DownloadSignedDocumentAsync(
                            accessToken,
                            request.PackageId.Value);

                        var signedPath = BuildSignedDocumentPath(request.OriginalDocumentPath);

                        await File.WriteAllBytesAsync(signedPath, signedBytes, cancellationToken);

                        await _signingRequestService.MarkSignedAsync(
                            request.Id,
                            signedPath,
                            cancellationToken);

                        result.CompletedCaseLetterIds.Add(request.CaseLetterId);
                    }
                    else if (status.IsRejected)
                    {
                        var reason = status.StatusDescription ?? "Declined by the Manager in SigningHub.";

                        await _signingRequestService.MarkFailedAsync(
                            request.Id,
                            reason,
                            cancellationToken);

                        result.RejectedLetters.Add(new RejectedLetter
                        {
                            CaseLetterId = request.CaseLetterId,
                            Reason = reason
                        });
                    }

                    // Otherwise still pending - nothing to do this pass.
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to check SigningHub status for signing request {SigningRequestId}.",
                        request.Id);
                }
            }

            return result;
        }

        private string BuildSignedDocumentPath(string originalDocumentPath)
        {
            var rootFolder = _configuration["Documents:RootFolder"] ?? "Documents";
            var directory = Path.Combine(rootFolder, "Signed");

            Directory.CreateDirectory(directory);

            var fileName = "Signed_" + Path.GetFileName(originalDocumentPath);

            return Path.Combine(directory, fileName);
        }
    }
}