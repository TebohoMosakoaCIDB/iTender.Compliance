using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IComplianceCaseWorkflowService
    {
        Task IssueInstructionalLetterAsync(Guid caseId, Guid findingId, CancellationToken ct = default);
        Task IssueContraventionNoticeAsync(Guid caseId, Guid findingId, CancellationToken ct = default);
        Task IssueErratumAsync(Guid caseId, Guid findingId, CancellationToken ct = default);
        Task SubmitForManagerApprovalAsync(Guid actionId, CancellationToken ct = default);
        Task ApproveCorrespondenceAsync(Guid actionId, string? comments, CancellationToken ct = default);
        Task RejectCorrespondenceAsync(Guid actionId, string reason, CancellationToken ct = default);
    }
}
