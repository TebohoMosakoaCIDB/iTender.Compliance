using iTender.Application.DTOs;
using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IComplianceProcessingService
    {
        Task<Guid?> ProcessTenderAsync(
          Tender tender,
          HashSet<string> iTenderNumbers,
          IReadOnlyDictionary<string, Tender> iTenderLookup,   // NEW: key = normalized employer+title
          List<ContractModel> crmContracts,
          Guid syncId,
          Guid? userId,
          CancellationToken cancellationToken = default);
    }
}
