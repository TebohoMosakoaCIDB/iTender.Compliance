using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class PublicHolidayRepository
        : RepositoryBase, IPublicHolidayRepository
    {
        public PublicHolidayRepository(ComplianceDbContext context)
            : base(context)
        {
        }

        public async Task<HashSet<DateOnly>> GetAllDatesAsync(
            CancellationToken cancellationToken = default)
        {
            var dates = await Context.PublicHolidays
                .Select(x => x.Date)
                .ToListAsync(cancellationToken);

            return dates.ToHashSet();
        }

        public Task<bool> ExistsForYearAsync(
            int year,
            CancellationToken cancellationToken = default)
        {
            return Context.PublicHolidays
                .AnyAsync(x => x.Date.Year == year, cancellationToken);
        }

        public async Task AddRangeAsync(
            IEnumerable<PublicHoliday> holidays,
            CancellationToken cancellationToken = default)
        {
            await Context.PublicHolidays.AddRangeAsync(holidays, cancellationToken);
        }
    }
}