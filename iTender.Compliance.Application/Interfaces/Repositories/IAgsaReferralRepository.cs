using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface IAgsaReferralRepository
    {
        Task<AGSAReferral?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<int> CountByStatusAsync(
            EnforcementReferralStatus status,
            CancellationToken cancellationToken = default);

        Task<AGSAReferral?> GetByCaseIdAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default);

        Task<List<AGSAReferral>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task AddAsync(
            AGSAReferral referral,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            AGSAReferral referral,
            CancellationToken cancellationToken = default);
    }
}
