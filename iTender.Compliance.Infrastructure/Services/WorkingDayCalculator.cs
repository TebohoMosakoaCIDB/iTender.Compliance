using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;

namespace iTender.Compliance.Infrastructure.Services
{
    /// <summary>
    /// Adds N working days (Mon-Fri) to a date. Per the finalized SLA rules: IL response =
    /// 2 working days, CN response = 14 working days, reminder = 7 working days into whichever
    /// letter is currently outstanding.
    ///
    /// NOTE: South African public holidays are not accounted for. If a statutory public-holiday
    /// calendar is required for legal accuracy, this is the single place to add it - inject an
    /// IPublicHolidayProvider here and skip those dates too.
    /// </summary>
    public class WorkingDayCalculator : IWorkingDayCalculator
    {
        public DateTime AddWorkingDays(DateTime start, int workingDays)
        {
            if (workingDays == 0)
                return start;

            var direction = workingDays > 0 ? 1 : -1;
            var remaining = Math.Abs(workingDays);
            var date = start;

            while (remaining > 0)
            {
                date = date.AddDays(direction);

                if (date.DayOfWeek != DayOfWeek.Saturday &&
                    date.DayOfWeek != DayOfWeek.Sunday)
                {
                    remaining--;
                }
            }

            return date;
        }
    }
}