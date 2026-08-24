using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class CaseLetterRepository : RepositoryBase, ICaseLetterRepository
    {
        public CaseLetterRepository(ComplianceDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<CaseLetter>> GetOverdueLettersAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await Context.CaseLetters
                .Include(l => l.ComplianceCase)
                .Where(l => l.ResponseDueOn < now && l.RespondedOn == null)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            CaseLetter letter,
            CancellationToken cancellationToken = default)
        {
            await Context.CaseLetters.AddAsync(letter, cancellationToken);
        }

        public Task UpdateAsync(
            CaseLetter letter,
            CancellationToken cancellationToken = default)
        {
            Context.CaseLetters.Update(letter);
            return Task.CompletedTask;
        }

        public Task<CaseLetter?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Context.CaseLetters
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<List<CaseLetter>> GetByComplianceCaseIdAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default)
        {
            return Context.CaseLetters
                .Where(x => x.ComplianceCaseId == complianceCaseId)
                .OrderBy(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public Task<List<CaseLetter>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Context.CaseLetters
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public Task<List<CaseLetter>> GetByCaseIdAsync(
    Guid complianceCaseId,
    CancellationToken cancellationToken = default)
        {
            return Context.CaseLetters
                .Where(x => x.ComplianceCaseId == complianceCaseId)
                .OrderBy(x => x.LetterNumber)
                .ToListAsync(cancellationToken);
        }

        public Task<CaseLetter?> GetLatestAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default)
        {
            return Context.CaseLetters
                .Where(x => x.ComplianceCaseId == complianceCaseId)
                .OrderByDescending(x => x.LetterNumber)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<List<CaseLetter>> GetOutstandingAsync(
            CancellationToken cancellationToken = default)
        {
            return Context.CaseLetters
                .Where(x =>
                    !x.RespondedOn.HasValue &&
                    x.ResponseDueOn <= DateTime.UtcNow)
                .OrderBy(x => x.ResponseDueOn)
                .ToListAsync(cancellationToken);
        }
    }
}
