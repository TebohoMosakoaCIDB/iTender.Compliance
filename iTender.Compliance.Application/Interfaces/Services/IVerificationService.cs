using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IVerificationService
    {
        Task<VerificationResult> RunAutomatedVerificationAsync(
            ComplianceCase @case,
            CancellationToken cancellationToken = default);

        Task<VerificationResult> ReviewVerificationAsync(
            Guid findingId,
            bool isConfirmed,
            string? overrideReason,
            Guid coUserId,
            CancellationToken cancellationToken = default);

        Task<VerificationResult> ManualVerifyAsync(
            Guid findingId,
            bool isCompliant,
            string? notes,
            Guid coUserId,
            CancellationToken cancellationToken = default);
    }

    public record VerificationResult(
        bool Success,
        VerificationStatus Status,
        string? Message,
        Guid? FindingId = null);
}
