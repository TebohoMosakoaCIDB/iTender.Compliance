using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class CaseNoteRepository : RepositoryBase, ICaseNoteRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        public CaseNoteRepository(ComplianceDbContext context, IUnitOfWork unitOfWork)
            : base(context)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task AddAsync(CaseNote note, CancellationToken cancellationToken = default)
        {
            await Context.Notes.AddAsync(note, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<CaseNote>> GetByCaseIdAsync(Guid complianceCaseId, CancellationToken cancellationToken = default)
        {
            return await Context.Notes
                .Where(x => x.ComplianceCaseId == complianceCaseId)
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

    }
}
