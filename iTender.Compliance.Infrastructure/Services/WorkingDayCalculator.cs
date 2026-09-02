using iTender.Compliance.Application.Interfaces.Repositories;

namespace iTender.Compliance.Infrastructure.Services
{
    /// <summary>
    /// Adds N working days (Mon-Fri, excluding South African public holidays - see
    /// PublicHolidaySeeder) to a date. Per the finalized SLA rules: IL response = 2 working
    /// days, CN response = 14 working days, reminder = 7 working days into whichever letter
    /// is currently outstanding.
    /// </summary>
    public class WorkingDayCalculator : IWorkingDayCalculator
    {
        private readonly IPublicHolidayRepository _publicHolidayRepository;

        public WorkingDayCalculator(IPublicHolidayRepository publicHolidayRepository)
        {
            _publicHolidayRepository = publicHolidayRepository;
        }

        public async Task<DateTime> AddWorkingDaysAsync(
            DateTime start,
            int workingDays,
            CancellationToken cancellationToken = default)
        {
            if (workingDays == 0)
                return start;

            var holidays = await _publicHolidayRepository.GetAllDatesAsync(cancellationToken);

            var direction = workingDays > 0 ? 1 : -1;
            var remaining = Math.Abs(workingDays);
            var date = start;

            while (remaining > 0)
            {
                date = date.AddDays(direction);

                var isWeekend =
                    date.DayOfWeek == DayOfWeek.Saturday ||
                    date.DayOfWeek == DayOfWeek.Sunday;

                var isHoliday = holidays.Contains(DateOnly.FromDateTime(date));

                if (!isWeekend && !isHoliday)
                    remaining--;
            }

            return date;
        }
    }
}