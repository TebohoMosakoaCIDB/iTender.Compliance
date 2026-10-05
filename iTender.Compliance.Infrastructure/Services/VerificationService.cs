using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly IComplianceFindingRepository _findingRepository;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;

        public VerificationService(
            IComplianceFindingRepository findingRepository,
            IAuditService auditService,
            INotificationService notificationService,
            IUnitOfWork unitOfWork)
        {
            _findingRepository = findingRepository;
            _auditService = auditService;
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
        }

        public async Task<VerificationResult> RunAutomatedVerificationAsync(
            ComplianceCase @case,
            CancellationToken cancellationToken = default)
        {
            // TODO: replace with real RoP/iTender lookup.
            // For now we trust the flag populated during sync.
            var tender = @case.Tender;
            var isRegistered = tender.IsRegisteredOnRoP;

            // No finding means the automated check passed.
            if (isRegistered)
            {
                await _auditService.LogAsync(
                    AuditAction.Updated,
                    AuditEntity.ComplianceCase,
                    @case.Id,
                    "Automated RoP verification passed. No finding raised.",
                    null,
                    cancellationToken);

                return new VerificationResult(
                    Success: true,
                    Status: VerificationStatus.AutomatedPassed,
                    Message: "Automated verification passed.",
                    FindingId: null);
            }

            // Failed - raise a finding for CO review.
            var finding = new ComplianceFinding
            {
                ComplianceCaseId = @case.Id,
                Stream = ComplianceStream.RopRegistration,
                FindingType = ComplianceFindingType.AwardNotRegisteredOnRop,
                Description = "Automated verification failed: award is not registered on the Register of Projects.",
                RegulatoryReference = "CIDB Act 38 of 2000, Reg 18",
                IdentifiedAt = DateTime.UtcNow,
                IsResolved = false,
                VerificationStatus = VerificationStatus.AutomatedFailed,
                TenderStatusAtCheck = MapTenderStatus(tender)
            };

            await _findingRepository.AddAsync(finding, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.Updated,
                AuditEntity.ComplianceFinding,
                finding.Id,
                "Automated RoP verification failed. Finding raised, awaiting CO review.",
                null,
                cancellationToken);

            // D04: notify the assigned CO (or fall back to the case's manager)
            await NotifyCoOfFailureAsync(@case, finding, cancellationToken);

            return new VerificationResult(
                Success: true,
                Status: VerificationStatus.AutomatedFailed,
                Message: "Automated verification failed. CO review required.",
                FindingId: finding.Id);
        }

        public async Task<VerificationResult> ReviewVerificationAsync(
            Guid findingId,
            bool isConfirmed,
            string? overrideReason,
            Guid coUserId,
            CancellationToken cancellationToken = default)
        {
            var finding = await _findingRepository.GetByIdAsync(findingId, cancellationToken);
            if (finding is null)
                return new VerificationResult(false, VerificationStatus.Pending, "Finding not found.");

            if (finding.VerificationStatus != VerificationStatus.AutomatedFailed)
                return new VerificationResult(false, finding.VerificationStatus,
                    "Finding is not awaiting CO review.");

            if (isConfirmed)
            {
                finding.VerificationStatus = VerificationStatus.CoConfirmed;
                finding.Description += $" | CO confirmed automated failure on {DateTime.UtcNow:O}.";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(overrideReason))
                    return new VerificationResult(false, finding.VerificationStatus,
                        "Override reason is required when overriding an automated failure.");

                // Override: automated check was wrong, treat finding as no longer valid.
                finding.VerificationStatus = VerificationStatus.CoOverridden;
                finding.IsResolved = true;
                finding.ResolvedOn = DateTime.UtcNow;
                finding.Description += $" | CO overrode automated failure on {DateTime.UtcNow:O}. Reason: {overrideReason}";
            }

            await _findingRepository.UpdateAsync(finding, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var auditAction = isConfirmed
                ? AuditAction.Updated
                : AuditAction.Updated;

            await _auditService.LogAsync(
                auditAction,
                AuditEntity.ComplianceFinding,
                finding.Id,
                isConfirmed
                    ? "CO confirmed automated verification failure."
                    : $"CO overrode automated verification failure. Reason: {overrideReason}",
                coUserId,
                cancellationToken);

            return new VerificationResult(
                true,
                finding.VerificationStatus,
                isConfirmed ? "Automated failure confirmed." : "Automated failure overridden.",
                finding.Id);
        }

        public async Task<VerificationResult> ManualVerifyAsync(
            Guid findingId,
            bool isCompliant,
            string? notes,
            Guid coUserId,
            CancellationToken cancellationToken = default)
        {
            var finding = await _findingRepository.GetByIdAsync(findingId, cancellationToken);
            if (finding is null)
                return new VerificationResult(false, VerificationStatus.Pending, "Finding not found.");

            if (isCompliant)
            {
                finding.VerificationStatus = VerificationStatus.ManualPassed;
                finding.IsResolved = true;
                finding.ResolvedOn = DateTime.UtcNow;
                finding.Description += $" | Manually verified compliant by CO on {DateTime.UtcNow:O}. Notes: {notes}";
            }
            else
            {
                finding.VerificationStatus = VerificationStatus.ManualFailed;
                finding.IsResolved = false;
                finding.Description += $" | Manually verified non-compliant by CO on {DateTime.UtcNow:O}. Notes: {notes}";
            }

            await _findingRepository.UpdateAsync(finding, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.Updated,
                AuditEntity.ComplianceFinding,
                finding.Id,
                isCompliant
                    ? $"CO manually verified compliance. Notes: {notes}"
                    : $"CO manually verified non-compliance. Notes: {notes}",
                coUserId,
                cancellationToken);

            return new VerificationResult(
                true,
                finding.VerificationStatus,
                isCompliant ? "Manual verification: compliant." : "Manual verification: non-compliant.",
                finding.Id);
        }

        private async Task NotifyCoOfFailureAsync(
            ComplianceCase @case,
            ComplianceFinding finding,
            CancellationToken cancellationToken)
        {
            // The CO is the assigned agent. If none, no notification target here -
            // D04's manual verification path handles the fallback.
            if (@case.Agent is null)
                return;

            await _notificationService.NotifyAsync(new CreateNotificationModel
            {
                UserId = @case.Agent.UserId,
                Title = "Verification failed - review required",
                Message = $"Automated RoP verification failed for tender {@case.Tender.TenderNumber}. Confirm, override or manually verify.",
                Type = NotificationType.Warning,
                Url = $"/cases/{@case.Id}/findings/{finding.Id}"
            });
        }

        private static TenderStatus MapTenderStatus(Tender tender)
        {
            // Tender.TenderStatus is a free-text string from the source.
            // Map to the enum your domain uses for "status at check".
            return string.Equals(tender.TenderStatus, "closed", StringComparison.OrdinalIgnoreCase)
                ? TenderStatus.Closed
                : TenderStatus.Open;
        }
    }
}
