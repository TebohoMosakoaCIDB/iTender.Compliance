using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.Services
{
    public class DuplicateVerificationService : IDuplicateVerificationService
    {
        private readonly IDuplicateGroupRepository _groups;
        private readonly IUnitOfWork _unitOfWork;

        public DuplicateVerificationService(
            IDuplicateGroupRepository groups,
            IUnitOfWork unitOfWork)
        {
            _groups = groups;
            _unitOfWork = unitOfWork;
        }

        public async Task ConfirmDuplicateAsync(
            Guid groupId,
            Guid coUserId,
            string? comment,
            CancellationToken cancellationToken = default)
        {
            var group = await _groups.GetByIdAsync(groupId, cancellationToken)
                ?? throw new InvalidOperationException("Duplicate group not found.");

            if (group.Status != DuplicateStatus.PendingCOVerification)
                throw new InvalidOperationException("Group is not pending CO verification.");

            group.Status = DuplicateStatus.ConfirmedDuplicate;
            group.ConfirmedByUserId = coUserId;
            group.ConfirmedOn = DateTime.UtcNow;
            group.CoComment = comment;
            group.ModifiedBy = coUserId;
            group.ModifiedOn = DateTime.UtcNow;

            await _groups.UpdateAsync(group, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task RejectDuplicateAsync(
            Guid groupId,
            Guid coUserId,
            string? comment,
            CancellationToken cancellationToken = default)
        {
            var group = await _groups.GetByIdAsync(groupId, cancellationToken)
                ?? throw new InvalidOperationException("Duplicate group not found.");

            if (group.Status != DuplicateStatus.PendingCOVerification)
                throw new InvalidOperationException("Group is not pending CO verification.");

            group.Status = DuplicateStatus.NotDuplicate;
            group.ConfirmedByUserId = coUserId;
            group.ConfirmedOn = DateTime.UtcNow;
            group.CoComment = comment;
            group.ModifiedBy = coUserId;
            group.ModifiedOn = DateTime.UtcNow;

            await _groups.UpdateAsync(group, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Re-processing of the candidate tenders is out of scope here -
            // it will be handled by the caller / a follow-up sync run once
            // the NotDuplicate decision is recorded.
        }
    }
}
