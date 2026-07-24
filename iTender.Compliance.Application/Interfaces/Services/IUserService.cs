using iTender.Compliance.Application.Common;
using iTender.Compliance.Application.DTOs;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<UpdateUserModel?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<ServiceResult> RegisterAsync(
            RegisterUserModel model,
            CancellationToken cancellationToken = default);

        Task<PagedResult<UserListModel>> SearchAsync(
            UserSearchModel search,
            CancellationToken cancellationToken = default);

        Task<List<UserListModel>> GetUsersWithoutAgentsAsync(
            CancellationToken cancellationToken = default);

        Task<ServiceResult> UpdateAsync(
            UpdateUserModel model,
            CancellationToken cancellationToken = default);
    }
}
