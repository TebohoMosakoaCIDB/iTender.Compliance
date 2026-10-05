using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IDuplicateDetectionService
    {
        Task<IReadOnlyList<DuplicateCandidate>> FindPotentialDuplicatesAsync(
            Tender tender,
            CancellationToken cancellationToken = default);
    }
}
