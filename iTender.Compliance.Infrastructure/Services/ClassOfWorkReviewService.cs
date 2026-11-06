using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class ClassOfWorkReviewService : IClassOfWorkReviewService
    {
        private readonly IComplianceFindingRepository _findingRepository;
        private readonly IComplianceCaseRepository _caseRepository;
        private readonly ITenderRepository _tenderRepository;
        private readonly IAuditService _auditService;

        public ClassOfWorkReviewService(
            IComplianceFindingRepository findingRepository,
            IComplianceCaseRepository caseRepository,
            ITenderRepository tenderRepository,
            IAuditService auditService)
        {
            _findingRepository = findingRepository;
            _caseRepository = caseRepository;
            _tenderRepository = tenderRepository;
            _auditService = auditService;
        }

        public async Task ResolveAsync(
            Guid findingId,
            string selectedClassOfWorks,
            string? notes,
            Guid officerUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(selectedClassOfWorks))
                throw new ArgumentException(
                    "A class of works must be selected.",
                    nameof(selectedClassOfWorks));

            var finding = await _findingRepository.GetByIdAsync(
                findingId,
                cancellationToken)
                ?? throw new InvalidOperationException(
                    "Compliance finding not found.");

            if (finding.Stream != ComplianceStream.ClassOfWorks)
                throw new InvalidOperationException(
                    "This finding is not a class-of-works finding.");

            if (finding.IsResolved)
                throw new InvalidOperationException(
                    "This finding has already been resolved.");

            var complianceCase = await _caseRepository.GetByIdAsync(
                finding.ComplianceCaseId,
                cancellationToken)
                ?? throw new InvalidOperationException(
                    "Compliance case not found.");

            var tender = complianceCase.Tender
                ?? throw new InvalidOperationException(
                    "The tender linked to this case could not be found.");

            var suggestedClass = tender.ClaimedClassOfWorks;

            var confirmedSuggestedClass =
                !string.IsNullOrWhiteSpace(suggestedClass) &&
                string.Equals(
                    suggestedClass,
                    selectedClassOfWorks.Trim(),
                    StringComparison.OrdinalIgnoreCase);

            var now = DateTime.UtcNow;

            // Preserve the automated suggestion.
            // The officer's accepted value becomes the actual class.
            tender.ClassOfWorks = selectedClassOfWorks.Trim();
            tender.ClassOfWorksSource = ClassOfWorksSource.ManuallySet;

            finding.IsResolved = true;
            finding.ResolvedOn = now;
            finding.VerificationStatus = confirmedSuggestedClass
                ? VerificationStatus.CoConfirmed
                : VerificationStatus.CoOverridden;

            finding.Description +=
                $" | Class-of-works reviewed by CO on {now:O}. " +
                $"Selected class: {tender.ClassOfWorks}. " +
                $"Decision: {(confirmedSuggestedClass ? "Suggested class confirmed" : "Class changed by CO")}. " +
                $"Notes: {notes ?? "None"}";

            await _tenderRepository.UpdateAsync(
                tender,
                cancellationToken);

            await _findingRepository.UpdateAsync(
                finding,
                cancellationToken);

            await _auditService.LogAsync(
                AuditAction.Updated,
                AuditEntity.ComplianceFinding,
                finding.Id,
                confirmedSuggestedClass
                    ? $"CO confirmed suggested class of works '{tender.ClassOfWorks}'. Notes: {notes}"
                    : $"CO selected class of works '{tender.ClassOfWorks}' instead of suggested class '{suggestedClass ?? "None"}'. Notes: {notes}",
                officerUserId,
                cancellationToken);
        }

        public async Task DeferAsync(
            Guid findingId,
            string reason,
            Guid officerUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException(
                    "A reason is required when deferring the review.",
                    nameof(reason));

            var finding = await _findingRepository.GetByIdAsync(
                findingId,
                cancellationToken)
                ?? throw new InvalidOperationException(
                    "Compliance finding not found.");

            if (finding.Stream != ComplianceStream.ClassOfWorks)
                throw new InvalidOperationException(
                    "This finding is not a class-of-works finding.");

            if (finding.IsResolved)
                throw new InvalidOperationException(
                    "This finding has already been resolved.");

            var now = DateTime.UtcNow;

            // Keep the finding open for further investigation.
            finding.Description +=
                $" | CO deferred class-of-works review on {now:O}. Reason: {reason.Trim()}";

            await _findingRepository.UpdateAsync(
                finding,
                cancellationToken);

            await _auditService.LogAsync(
                AuditAction.Updated,
                AuditEntity.ComplianceFinding,
                finding.Id,
                $"CO deferred class-of-works review. Reason: {reason.Trim()}",
                officerUserId,
                cancellationToken);
        }
    }
}