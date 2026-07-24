using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface IComplianceCaseRepository
    {
        Task<List<ComplianceCase>> GetCasesAwaitingReminderAsync(
            int reminderAfterHours,
            CancellationToken cancellationToken = default);

        Task<int> GetOpenCaseCountByAgentAsync(
    Guid agentId,
    CancellationToken cancellationToken = default);

        Task<ComplianceCase?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<CaseLetter?> GetLatestOutstandingAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default);

        Task<PagedResult<ComplianceCase>> SearchAsync(
            ComplianceCaseSearchModel search,
            CancellationToken cancellationToken = default);

        Task<List<ComplianceCase>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task AddAsync(
            ComplianceCase complianceCase,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            ComplianceCase complianceCase,
            CancellationToken cancellationToken = default);

        Task<ComplianceCaseDetailModel?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
