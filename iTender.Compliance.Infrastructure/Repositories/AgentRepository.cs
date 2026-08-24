using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class AgentRepository : RepositoryBase, IAgentRepository
    {
        public AgentRepository(ComplianceDbContext context)
            : base(context)
        {
        }

        public async Task AddAsync(
            Agent agent,
            CancellationToken cancellationToken = default)
        {
            await Context.Agents.AddAsync(agent, cancellationToken);
        }

        public Task UpdateAsync(
            Agent agent,
            CancellationToken cancellationToken = default)
        {
            Context.Agents.Update(agent);
            return Task.CompletedTask;
        }

        public Task<Agent?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Context.Agents
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<Agent?> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Context.Agents
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        }

        public Task<List<Agent>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Context.Agents
                .OrderBy(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public Task<List<Agent>> GetActiveAsync(
            CancellationToken cancellationToken = default)
        {
            var agents =  Context.Agents
                .Where(x => x.IsActive)
                .ToListAsync(cancellationToken);

            return agents;
        }

        public async Task<List<AgentLookupModel>> GetLookupAsync(
    CancellationToken cancellationToken = default)
        {
            return await Context.Agents
                .Where(x => x.IsActive)
                .OrderBy(x => x.FullName)
                .Select(x => new AgentLookupModel
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    UserId = x.UserId,
                    OpenCases = x.ComplianceCases.Count(c => c.Status != CaseStatus.Closed)
                })
                .ToListAsync(cancellationToken);
        }
    }
}
