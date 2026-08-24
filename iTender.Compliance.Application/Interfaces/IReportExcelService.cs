namespace iTender.Compliance.Application.Interfaces
{
    public interface IReportExcelService
    {
        Task<byte[]> GenerateAsync(
            DateTime fromDate,
            DateTime toDate);
    }
}
