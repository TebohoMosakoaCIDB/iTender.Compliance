namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface IWorkingDayCalculator
    {
        Task<DateTime> AddWorkingDaysAsync(
             DateTime start,
             int workingDays,
             CancellationToken cancellationToken = default);
    }
}
