using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class LetterNumberGenerator : ILetterNumberGenerator
    {
        private readonly ISystemSettingRepository _settingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public LetterNumberGenerator(ISystemSettingRepository settingRepository, IUnitOfWork unitOfWork)
        {
            _settingRepository = settingRepository;
            _unitOfWork = unitOfWork;
        }

        public Task<int> GetNextNumberAsync(LetterType letterType, CancellationToken cancellationToken = default)
        {
            // Temporary: return a random or static number
            return Task.FromResult(new Random().Next(1000, 9999));
        }
    }
}
