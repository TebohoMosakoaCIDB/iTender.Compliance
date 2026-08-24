using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task AddAsync(
            Notification notification,
            CancellationToken cancellationToken = default);

        Task<List<Notification>> GetUnreadAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<List<Notification>> GetRecentAsync(
            Guid userId,
            int count = 20,
            CancellationToken cancellationToken = default);

        Task<int> GetUnreadCountAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task MarkAsReadAsync(
            Guid notificationId,
            CancellationToken cancellationToken = default);

        Task MarkAllAsReadAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<List<Notification>> GetAllAsync(
            Guid userId,
            bool? unreadOnly = null,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            Guid notificationId,
            CancellationToken cancellationToken = default);

        Task<NotificationDetailModel?> GetByIdAsync(Guid id);
    }
}
