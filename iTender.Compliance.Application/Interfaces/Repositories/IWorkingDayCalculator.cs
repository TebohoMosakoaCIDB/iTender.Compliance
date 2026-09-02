namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface IWorkingDayCalculator
    {
        DateTime AddWorkingDays(DateTime start, int workingDays);
    }
}
