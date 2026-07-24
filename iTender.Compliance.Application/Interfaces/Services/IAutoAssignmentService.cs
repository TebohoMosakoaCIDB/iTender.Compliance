using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IAutoAssignmentService
    {
        Task<Agent?> SelectAgentAsync(
        ComplianceCase complianceCase,
        CaseDistributionMethod method,
        CancellationToken cancellationToken = default);

        Task<List<ComplianceCase>> GetUnassignedCasesAsync(CancellationToken cancellationToken = default);

        Task AssignUnassignedCasesAsync(CancellationToken cancellationToken = default);
    }
}
