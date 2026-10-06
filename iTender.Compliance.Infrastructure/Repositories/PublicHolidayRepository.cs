using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class PublicHolidayRepository : RepositoryBase, IPublicHolidayRepository
    {
        public PublicHolidayRepository(IDbContextFactory<ComplianceDbContext> contextFactory)
            : base(contextFactory)
        {
        }

        public async Task<HashSet<DateOnly>> GetAllDatesAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var dates = await context.PublicHolidays
                .Select(x => x.Date)
                .ToListAsync(cancellationToken);

            return dates.ToHashSet();
        }

        public async Task<bool> ExistsForYearAsync(
            int year,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.PublicHolidays
                .AnyAsync(x => x.Date.Year == year, cancellationToken);
        }

        public async Task AddRangeAsync(
            IEnumerable<PublicHoliday> holidays,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            await context.PublicHolidays.AddRangeAsync(holidays, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}