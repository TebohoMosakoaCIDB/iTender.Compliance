
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk.PluginTelemetry;

namespace iTender.Compliance.Infrastructure.Services
{
    public class TenderClassificationService : ITenderClassificationService
    {
        private readonly ITenderRepository _tenderRepository;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISynchronizationService _synchronizationService;
        private readonly ILogger<TenderClassificationService> _logger;

        public TenderClassificationService(
            ISynchronizationService synchronizationService,
            ITenderRepository tenderRepository,
            IAuditService auditService,
            IUnitOfWork unitOfWork,
            ILogger<TenderClassificationService>? logger)
        {
            _synchronizationService = synchronizationService;
            _tenderRepository = tenderRepository;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ConfirmAsync(
            Guid tenderId,
            Guid userId,
            string? notes,
            CancellationToken cancellationToken = default)
        {
            var tender = await _tenderRepository.GetByIdAsync(tenderId, cancellationToken)
                ?? throw new InvalidOperationException($"Tender {tenderId} not found.");

            tender.ClassificationReviewedOn = DateTime.UtcNow;
            tender.ClassificationReviewedByUserId = userId;
            tender.ClassificationReviewNotes = notes;
            tender.ClassificationOverridden = false;
            tender.ClassificationStatus = tender.IsConstruction
                ? ClassificationStatus.Confirmed
                : ClassificationStatus.Rejected;

            await _tenderRepository.UpdateAsync(tender, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.Updated,
                AuditEntity.Tender,
                tender.Id,
                $"Tender classification confirmed as " +
                $"{(tender.IsConstruction ? "construction-related" : "non-construction")} by CO.",
                userId,
                cancellationToken);

            if (tender.ClassificationStatus == ClassificationStatus.Confirmed)
            {
                await _synchronizationService.ProcessClassifiedTenderAsync(
                    tender.Id,
                    userId,
                    cancellationToken);
            }
        }

        public async Task<int> ConfirmManyAsync(
    IEnumerable<Guid> tenderIds,
    Guid userId,
    string? notes,
    CancellationToken cancellationToken = default)
        {
            var ids = tenderIds?
                .Distinct()
                .ToList() ?? new List<Guid>();

            if (ids.Count == 0)
                return 0;

            var reviewedTenderIds = await _tenderRepository
                .MarkClassificationReviewedAsync(
                    ids,
                    userId,
                    notes,
                    cancellationToken);

            var confirmedCount = reviewedTenderIds.Count;

            if (confirmedCount == 0)
                return 0;

            await _auditService.LogAsync(
                AuditAction.Updated,
                AuditEntity.Tender,
                Guid.Empty,
                $"Bulk classification confirmation: {confirmedCount} tender(s) reviewed.",
                userId,
                cancellationToken);

            foreach (var tenderId in reviewedTenderIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var tender = await _tenderRepository.GetByIdAsync(
                        tenderId,
                        cancellationToken);

                    if (tender is null ||
                        tender.ClassificationStatus != ClassificationStatus.Confirmed)
                    {
                        continue;
                    }

                    await _synchronizationService.ProcessClassifiedTenderAsync(
                        tender.Id,
                        userId,
                        cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Compliance processing failed for tender {TenderId} during bulk classification confirmation.",
                        tenderId);

                    // Continue processing the remaining tenders.
                }
            }

            return confirmedCount;
        }

        public async Task OverrideAsync(
            Guid tenderId,
            bool isConstruction,
            Guid userId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            var tender = await _tenderRepository.GetByIdAsync(tenderId, cancellationToken)
                ?? throw new InvalidOperationException("Tender not found.");

            var previous = tender.IsConstruction;

            tender.IsConstruction = isConstruction;
            tender.ClassificationReviewedByUserId = userId;
            tender.ClassificationReviewedOn = DateTime.UtcNow;
            tender.ClassificationReviewNotes = reason;
            tender.ClassificationOverridden = true;
            tender.ClassificationStatus = ClassificationStatus.Overridden;

            await _tenderRepository.UpdateAsync(tender, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.Updated,
                AuditEntity.Tender,
                tender.Id,
                $"Tender classification overridden from " +
                $"{(previous ? "construction" : "non-construction")} to " +
                $"{(isConstruction ? "construction" : "non-construction")}. Reason: {reason}",
                userId,
                cancellationToken);

            if (isConstruction)
            {
                await _synchronizationService.ProcessClassifiedTenderAsync(
                    tender.Id,
                    userId,
                    cancellationToken);
            }
        }
    }
}