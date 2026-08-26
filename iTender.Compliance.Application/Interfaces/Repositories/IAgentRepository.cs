using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface IAgentRepository
    {
        Task<Agent?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<Agent?> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<List<Agent>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<List<Agent>> GetActiveAsync(
            CancellationToken cancellationToken = default);

        Task<List<Agent>> GetManagersAsync(
           CancellationToken cancellationToken = default);

        Task AddAsync(
            Agent agent,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            Agent agent,
            CancellationToken cancellationToken = default);

        Task<List<AgentLookupModel>> GetLookupAsync(
            CancellationToken cancellationToken = default);
    }
}
