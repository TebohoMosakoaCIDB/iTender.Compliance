namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IReportPdfService
    {
        Task<byte[]> GenerateAsync(
            DateTime fromDate,
            DateTime toDate);
    }
}
