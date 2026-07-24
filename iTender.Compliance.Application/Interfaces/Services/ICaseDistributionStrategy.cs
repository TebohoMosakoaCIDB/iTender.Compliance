using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface ICaseDistributionStrategy
    {
        CaseDistributionMethod Method { get; }
        Task<Agent?> SelectAgentAsync(
            ComplianceCase complianceCase,
            CancellationToken cancellationToken = default);
    }
}
