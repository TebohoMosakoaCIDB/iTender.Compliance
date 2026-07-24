using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IComplianceService
    {
        Task AssignAgentAsync(
            Guid complianceCaseId,
            Guid agentId,
            CasePriority priority,
            string? comments,
            CancellationToken cancellationToken = default);

        Task MarkCompliantAsync(
            Guid complianceCaseId,
            string? notes,
            CancellationToken cancellationToken = default);

        Task MarkNonCompliantAsync(
            Guid complianceCaseId,
            string? notes,
            CancellationToken cancellationToken = default);

        Task CloseCaseAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default);        
    }
}
