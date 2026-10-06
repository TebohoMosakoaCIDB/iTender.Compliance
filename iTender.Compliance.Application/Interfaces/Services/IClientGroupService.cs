using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IClientGroupService
    {
        /// <summary>
        /// Sets the group owner from the first assigned case, if not already set.
        /// Called after a case in the group is assigned.
        /// </summary>
        Task SetOwnerFromCaseAsync(
            Guid groupId,
            Guid complianceCaseId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the group's owner, or null if unassigned.
        /// </summary>
        Task<Guid?> GetOwnerAsync(
            Guid groupId,
            CancellationToken cancellationToken = default);

        /// <summary>Manually reassigns a group and all cases in it to a different agent.</summary>
        Task ManuallyAssignGroupAsync(
            Guid groupId,
            Guid agentId,
            Guid managerUserId,
            string? comment,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds the draft group for the employer, or creates one.
        /// Called automatically when a case is created.
        /// </summary>
        Task<ClientGroup> GetOrCreateDraftGroupAsync(
            string employerName,
            string? contactName,
            string? contactEmail,
            string? ccEmails,
            CancellationToken cancellationToken = default);

        /// <summary>Adds a case to the draft group (idempotent).</summary>
        Task AddCaseAsync(
            Guid groupId,
            Guid complianceCaseId,
            CancellationToken cancellationToken = default);

        /// <summary>Removes a case from a draft group.</summary>
        Task RemoveCaseAsync(
            Guid groupId,
            Guid complianceCaseId,
            CancellationToken cancellationToken = default);

        /// <summary>Generates the consolidated letter + Annexure A, then emails them.</summary>
        Task<Guid> IssueGroupAsync(
            Guid groupId,
            LetterType letterType,
            Guid issuedByUserId,
            CancellationToken cancellationToken = default);

        Task UpdateRecipientAsync(
            Guid groupId,
            string? contactName,
            string? contactEmail,
            string? ccEmails,
            Guid modifiedByUserId,
            CancellationToken cancellationToken = default);
        }
}