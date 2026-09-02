using iTender.Compliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Data
{
    /// <summary>
    /// Seeds South African public holidays for working-day calculations. Safe to run on every
    /// startup - only adds years that aren't already covered, so it self-extends over time as
    /// the current year advances rather than needing a manual data update.
    /// </summary>
    public static class PublicHolidaySeeder
    {
        public static async Task SeedAsync(ComplianceDbContext context)
        {
            var currentYear = DateTime.UtcNow.Year;

            // Keep last year on hand (in-flight cases spanning New Year) through two years ahead.
            for (var year = currentYear - 1; year <= currentYear + 2; year++)
            {
                var alreadySeeded = await context.PublicHolidays
                    .AnyAsync(x => x.Date.Year == year);

                if (alreadySeeded)
                    continue;

                var holidays = BuildHolidaysForYear(year);

                await context.PublicHolidays.AddRangeAsync(holidays);
            }

            await context.SaveChangesAsync();
        }

        private static List<PublicHoliday> BuildHolidaysForYear(int year)
        {
            var easterSunday = CalculateEasterSunday(year);

            var fixedHolidays = new List<(DateOnly Date, string Name)>
            {
                (new DateOnly(year, 1, 1), "New Year's Day"),
                (new DateOnly(year, 3, 21), "Human Rights Day"),
                (easterSunday.AddDays(-2), "Good Friday"),
                (easterSunday.AddDays(1), "Family Day"),
                (new DateOnly(year, 4, 27), "Freedom Day"),
                (new DateOnly(year, 5, 1), "Workers' Day"),
                (new DateOnly(year, 6, 16), "Youth Day"),
                (new DateOnly(year, 8, 9), "National Women's Day"),
                (new DateOnly(year, 9, 24), "Heritage Day"),
                (new DateOnly(year, 12, 16), "Day of Reconciliation"),
                (new DateOnly(year, 12, 25), "Christmas Day"),
                (new DateOnly(year, 12, 26), "Day of Goodwill")
            };

            var holidays = new List<PublicHoliday>();

            foreach (var (date, name) in fixedHolidays)
            {
                holidays.Add(new PublicHoliday
                {
                    Date = date,
                    Name = name,
                    IsObservedShift = false
                });

                // Public Holidays Act, 1994: whenever a public holiday falls on a Sunday,
                // the following Monday becomes a public holiday too.
                if (date.DayOfWeek == DayOfWeek.Sunday)
                {
                    holidays.Add(new PublicHoliday
                    {
                        Date = date.AddDays(1),
                        Name = $"{name} (observed)",
                        IsObservedShift = true
                    });
                }
            }

            return holidays;
        }

        /// <summary>Anonymous Gregorian (Meeus/Jones/Butcher) algorithm - accurate for any Gregorian
        /// calendar year, which is all we need since South Africa has always used it.</summary>
        private static DateOnly CalculateEasterSunday(int year)
        {
            var a = year % 19;
            var b = year / 100;
            var c = year % 100;
            var d = b / 4;
            var e = b % 4;
            var f = (b + 8) / 25;
            var g = (b - f + 1) / 3;
            var h = (19 * a + b - d - g + 15) % 30;
            var i = c / 4;
            var k = c % 4;
            var l = (32 + 2 * e + 2 * i - h - k) % 7;
            var m = (a + 11 * h + 22 * l) / 451;
            var month = (h + l - 7 * m + 114) / 31;
            var day = ((h + l - 7 * m + 114) % 31) + 1;

            return new DateOnly(year, month, day);
        }
    }
}