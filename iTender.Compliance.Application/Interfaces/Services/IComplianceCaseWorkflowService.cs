//using iTender.Compliance.Domain.Enums;

//namespace iTender.Compliance.Application.Interfaces.Services
//{
//    public interface IComplianceCaseWorkflowService
//    {
//        Task AssignCaseAsync(
//            Guid caseId,
//            Guid agentId,
//            CancellationToken cancellationToken = default);

//        Task IssueErratumAsync(
//            Guid caseId,
//            CancellationToken cancellationToken = default);

//        Task IssueInstructionAsync(
//            Guid caseId,
//            CancellationToken cancellationToken = default);

//        Task IssueContraventionNoticeAsync(
//            Guid caseId,
//            CancellationToken cancellationToken = default);

//        Task IssueReminderAsync(
//            Guid caseId,
//            CancellationToken cancellationToken = default);

//        Task RecordResponseAsync(
//            Guid caseId,
//            string? comments = null,
//            CancellationToken cancellationToken = default);

//        Task RequestExtensionAsync(
//            Guid caseId,
//            string? comments = null,
//            CancellationToken cancellationToken = default);

//        Task GrantExtensionAsync(
//            Guid caseId,
//            DateTime newDueDate,
//            string? comments = null,
//            CancellationToken cancellationToken = default);

//        Task RaiseObjectionAsync(
//            Guid caseId,
//            string? comments = null,
//            CancellationToken cancellationToken = default);

//        Task EscalateToAGSAAsync(
//            Guid caseId,
//            string? comments = null,
//            CancellationToken cancellationToken = default);

//        Task CloseCaseAsync(
//            Guid caseId,
//            ComplianceOutcome outcome,
//            string? comments = null,
//            CancellationToken cancellationToken = default);
//    }
//}
