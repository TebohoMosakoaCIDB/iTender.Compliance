using iTender.Compliance.Application.DTOs;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IClientGroupedReportPdfService
    {
        Task<byte[]> GenerateAsync(Guid? agentId, CancellationToken ct = default);
    }

    public interface IClientGroupedReportExcelService
    {
        Task<byte[]> GenerateAsync(Guid? agentId, CancellationToken ct = default);
    }

    public interface INonCompliantCasesPdfService
    {
        Task<byte[]> GenerateAsync(NonCompliantCaseReportFilter filter, CancellationToken ct = default);
    }

    public interface INonCompliantCasesExcelService
    {
        Task<byte[]> GenerateAsync(NonCompliantCaseReportFilter filter, CancellationToken ct = default);
    }
}
