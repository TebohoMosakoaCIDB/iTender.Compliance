using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface IPublicHolidayRepository
    {
        /// <summary>All holiday dates on record, as a set for fast lookup.</summary>
        Task<HashSet<DateOnly>> GetAllDatesAsync(
            CancellationToken cancellationToken = default);

        Task<bool> ExistsForYearAsync(
            int year,
            CancellationToken cancellationToken = default);

        Task AddRangeAsync(
            IEnumerable<PublicHoliday> holidays,
            CancellationToken cancellationToken = default);
    }
}