using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface IClientGroupRepository
    {
        Task<ClientGroupSummaryModel?> GetSummaryByCaseIdAsync(
            Guid caseId,
            CancellationToken cancellationToken = default);
        Task AddAsync(ClientGroup group, CancellationToken cancellationToken = default);
        Task UpdateAsync(ClientGroup group, CancellationToken cancellationToken = default);
        Task<ClientGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<int> GetOpenGroupCountByAgentAsync(
            Guid agentId,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ClientGroup>> GetByAgentAsync(
            Guid agentId,
            CancellationToken cancellationToken = default);
        Task<ClientGroup?> GetDraftByEmployerAsync(
            string normalizedEmployerName,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ClientGroup>> GetByEmployerAsync(
            string normalizedEmployerName,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ClientGroup>> GetDraftAsync(
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ClientGroup>> GetIssuedAsync(
            CancellationToken cancellationToken = default);
        Task<PagedResult<ClientGroupListItemModel>> SearchAsync(
            ClientGroupSearchModel search,
            CancellationToken cancellationToken = default);
        Task<ClientGroupDetailModel?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default);
        Task<List<ComplianceCase>> GetEligibleCasesForEmployerAsync(
            string normalizedEmployerName,
            Guid excludeGroupId,
            CancellationToken cancellationToken = default);
        Task UpdateGroupMetaAsync(
            Guid groupId,
            string? contactName,
            string? contactEmail,
            string? ccEmails,
            Guid modifiedByUserId,
            CancellationToken cancellationToken = default);
    }
}
