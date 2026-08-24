using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface ILetterNumberGenerator
    {
        Task<int> GetNextNumberAsync(LetterType letterType, CancellationToken cancellationToken = default);
    }
}
