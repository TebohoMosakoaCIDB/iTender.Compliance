using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Enums;
using Microsoft.EntityFrameworkCore.Internal;

namespace iTender.Compliance.Infrastructure.Services
{
    public class TenderClassificationService : ITenderClassificationService
    {
        private readonly ITenderRepository _tenderRepository;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;

        public TenderClassificationService(
            ITenderRepository tenderRepository,
            IAuditService auditService,
            IUnitOfWork unitOfWork)
        {
            _tenderRepository = tenderRepository;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
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
        }

        public async Task<int> ConfirmManyAsync(
            IEnumerable<Guid> tenderIds,
            Guid userId,
            string? notes,
            CancellationToken cancellationToken = default)
        {
            var confirmed = await _tenderRepository.MarkClassificationReviewedAsync(
                tenderIds,
                userId,
                notes,
                cancellationToken);

            if (confirmed > 0)
            {
                await _auditService.LogAsync(
                    AuditAction.Updated,
                    AuditEntity.Tender,
                    Guid.Empty,
                    $"Bulk classification confirmation: {confirmed} tender(s) confirmed.",
                    userId,
                    cancellationToken);
            }

            return confirmed;
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
        }
    }
}