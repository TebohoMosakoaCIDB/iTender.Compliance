using iTender.Application.DTOs;
using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IComplianceProcessingService
    {
        Task<Guid?> ProcessTenderAsync(
            Tender tender,
            HashSet<string> iTenderNumbers,
            List<ContractModel> crmContracts,
            Guid syncId,
            Guid? userId,
            CancellationToken cancellationToken = default);
    }
}
