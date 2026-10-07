using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class CaseLetterRepository : RepositoryBase, ICaseLetterRepository
    {
        public CaseLetterRepository(IDbContextFactory<ComplianceDbContext> contextFactory)
            : base(contextFactory)
        {
        }

        public async Task<(int Received, int Outstanding)> CountResponsesAsync(
            DateTime now,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var received = await context.CaseLetters
                .CountAsync(l => l.RespondedOn != null, cancellationToken);

            var outstanding = await context.CaseLetters
                .CountAsync(l =>
                    l.RespondedOn == null &&
                    l.ResponseDueOn > now,
                    cancellationToken);

            return (received, outstanding);
        }

        public async Task AddAsync(
            CaseLetter letter,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            await context.CaseLetters.AddAsync(letter, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(
            CaseLetter letter,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            context.CaseLetters.Update(letter);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<CaseLetter?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.CaseLetters
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<CaseLetter>> GetByComplianceCaseIdAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.CaseLetters
                .Where(x => x.ComplianceCaseId == complianceCaseId)
                .OrderBy(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CaseLetter>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.CaseLetters
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CaseLetter>> GetByCaseIdAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.CaseLetters
                .Where(x => x.ComplianceCaseId == complianceCaseId)
                .OrderBy(x => x.LetterNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<CaseLetter?> GetLatestAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.CaseLetters
                .Where(x => x.ComplianceCaseId == complianceCaseId)
                .OrderByDescending(x => x.LetterNumber)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<CaseLetter>> GetOutstandingAsync(
    CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.CaseLetters
                .Include(x => x.ComplianceCase)
                    .ThenInclude(c => c.ComplianceFindings)
                .Where(x =>
                    !x.RespondedOn.HasValue &&
                    x.ResponseDueOn <= DateTime.UtcNow)
                .OrderBy(x => x.ResponseDueOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CaseLetter>> GetOutstandingWithCaseAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.CaseLetters
                .Include(x => x.ComplianceCase)
                    .ThenInclude(c => c.Tender)
                .Include(x => x.ComplianceCase)
                    .ThenInclude(c => c.Agent)
                .Include(x => x.ComplianceCase)
                    .ThenInclude(c => c.ComplianceFindings)
                .Where(x =>
                    !x.RespondedOn.HasValue &&
                    x.ResponseDueOn <= DateTime.UtcNow &&
                    x.ComplianceCase.Status != Domain.Enums.CaseStatus.Closed)
                .OrderBy(x => x.ResponseDueOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CaseLetter>> GetActiveWithCaseAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.CaseLetters
                .Include(x => x.ComplianceCase)
                    .ThenInclude(c => c.Tender)
                .Include(x => x.ComplianceCase)
                    .ThenInclude(c => c.Agent)
                .Include(x => x.ComplianceCase)
                    .ThenInclude(c => c.CaseLetters)
                .Where(x =>
                    !x.RespondedOn.HasValue &&
                    x.ComplianceCase.Status != Domain.Enums.CaseStatus.Closed)
                .ToListAsync(cancellationToken);
        }
    }
}