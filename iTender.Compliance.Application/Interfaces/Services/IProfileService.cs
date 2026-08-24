using iTender.Compliance.Application.DTOs;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IProfileService
    {
        Task<ProfileModel?> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

        Task<bool> UpdateAsync(
            Guid userId,
            UpdateMyProfileModel model,
            CancellationToken cancellationToken = default);
    }
}
