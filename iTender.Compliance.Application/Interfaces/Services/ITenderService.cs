using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface ITenderService
    {
        Task<List<TenderDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<TenderDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<TenderDetailModelDto?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<PagedResult<TenderDto>> SearchAsync(
            TenderSearchModel search,
            CancellationToken cancellationToken = default);
    }
}
