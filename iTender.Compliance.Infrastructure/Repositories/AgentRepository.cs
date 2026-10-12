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
        public AgentRepository(IDbContextFactory<ComplianceDbContext> contextFactory)
            : base(contextFactory)
        {
        }

        public async Task<Guid?> GetAgentIdByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            await using var context = await CreateContextAsync();
            return await context.Agents
                .Where(a => a.UserId == userId)
                .Select(a => (Guid?)a.Id)
                .FirstOrDefaultAsync(ct);
        }

        public async Task AddAsync(
            Agent agent,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            await context.Agents.AddAsync(agent, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(
            Agent agent,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            context.Agents.Update(agent);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Agent?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Agents
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<Agent?> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Agents
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        }

        public async Task<List<Agent>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Agents
                .OrderBy(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Agent>> GetActiveAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Agents
                .Where(x => x.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Agent>> GetManagersAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Agents
                .Where(x => x.IsActive && x.IsManager)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<AgentLookupModel>> GetLookupAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Agents
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

        public async Task<Guid?> GetUserIdByAgentIdAsync(Guid agentId, CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Agents
                .Where(a => a.Id == agentId && a.IsActive)
                .Select(a => (Guid?)a.UserId)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}