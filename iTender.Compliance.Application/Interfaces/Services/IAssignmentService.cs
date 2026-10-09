using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IAssignmentService
    {
        Task AssignAsync(ComplianceCase @case, CancellationToken cancellationToken = default);

        Task ManuallyAssignAsync(
            Guid caseId,
            Guid agentId,
            Guid managerUserId,
            string? comment,
            CancellationToken cancellationToken = default);

        Task NotifyManagersManualAssignmentRequiredAsync(
    ComplianceCase @case,
    string reason,
    CancellationToken cancellationToken = default);
    }
}
