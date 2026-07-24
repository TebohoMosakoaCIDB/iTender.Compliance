using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class ComplianceService : IComplianceService
    {
        private readonly IComplianceCaseRepository _complianceCaseRepository;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public ComplianceService(
            IComplianceCaseRepository complianceCaseRepository,
            IAuditService auditService,
            INotificationService notificationService,
            ICurrentUserService currentUser,
            IUnitOfWork unitOfWork)
        {
            _complianceCaseRepository = complianceCaseRepository;
            _notificationService = notificationService;
            _auditService = auditService;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task AssignAgentAsync(
            Guid complianceCaseId,
    Guid agentId,
    CasePriority priority,
    string? comments,
    CancellationToken cancellationToken = default)
        {
            var complianceCase = await _complianceCaseRepository.GetByIdAsync(
                complianceCaseId,
                cancellationToken);

            if (complianceCase == null)
                throw new InvalidOperationException("Compliance case not found.");

            complianceCase.AgentId = agentId;
            complianceCase.Status = CaseStatus.Assigned;
            complianceCase.ModifiedOn = DateTime.UtcNow;
            complianceCase.Priority = priority;
            complianceCase.Comments = comments;
            complianceCase.AssignedOn = DateTime.UtcNow;

            await _complianceCaseRepository.UpdateAsync(complianceCase, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                 AuditAction.Assigned,
                 AuditEntity.ComplianceCase,
                 complianceCaseId,
                $"Case assigned. Priority set to {priority}.",
                _currentUser.UserId,
                cancellationToken);

            await _notificationService.NotifyAsync(new CreateNotificationModel
            {
                UserId = agentId,
                Title = "New Case Assigned",
                Message = $"Tender {complianceCase.Tender.TenderNumber} has been assigned to you.",
                Type = NotificationType.Information,
                Url = $"/cases/{complianceCase.Id}"
            });
        }

        public async Task MarkCompliantAsync(
            Guid complianceCaseId,
            string? comments,
            CancellationToken cancellationToken = default)
        {
            var complianceCase = await _complianceCaseRepository.GetByIdAsync(
                complianceCaseId,
                cancellationToken);

            if (complianceCase == null)
                throw new InvalidOperationException("Compliance case not found.");

            complianceCase.Outcome = ComplianceOutcome.Compliant;
            complianceCase.Comments = comments;
            complianceCase.ModifiedOn = DateTime.UtcNow;

            await _complianceCaseRepository.UpdateAsync(complianceCase, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.Updated,
                AuditEntity.ComplianceCase,
                complianceCase.Id,
                "Case marked as compliant.",
                cancellationToken: cancellationToken);
        }

        public async Task MarkNonCompliantAsync(
            Guid complianceCaseId,
            string? comments,
            CancellationToken cancellationToken = default)
        {
            var complianceCase = await _complianceCaseRepository.GetByIdAsync(
                complianceCaseId,
                cancellationToken);

            if (complianceCase == null)
                throw new InvalidOperationException("Compliance case not found.");

            complianceCase.Outcome = ComplianceOutcome.NonCompliant;
            complianceCase.Comments = comments;
            complianceCase.ModifiedOn = DateTime.UtcNow;

            await _complianceCaseRepository.UpdateAsync(complianceCase, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.Updated,
                AuditEntity.ComplianceCase,
                complianceCase.Id,
                "Case marked as non-compliant.",
                cancellationToken: cancellationToken);
        }

        public async Task CloseCaseAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default)
        {
            var complianceCase = await _complianceCaseRepository.GetByIdAsync(
                complianceCaseId,
                cancellationToken);

            if (complianceCase == null)
                throw new InvalidOperationException("Compliance case not found.");

            complianceCase.Status = CaseStatus.Closed;
            complianceCase.ClosedDate = DateTime.UtcNow;
            complianceCase.ModifiedOn = DateTime.UtcNow;

            await _complianceCaseRepository.UpdateAsync(complianceCase, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.CaseClosed,
                AuditEntity.ComplianceCase,
                complianceCase.Id,
                "Compliance case closed.",
                cancellationToken: cancellationToken);
        }
    }
}
