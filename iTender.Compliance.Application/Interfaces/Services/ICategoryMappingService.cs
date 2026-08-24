namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface ICategoryMappingService
    {
        Task<(bool IsConstruction, string? ClassOfWork)> MapCategoryAsync(string? categoryName, CancellationToken cancellationToken = default);
    }
}
