using iTender.Compliance.Application.DTOs;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task NotifyAsync(
            CreateNotificationModel model,
            CancellationToken cancellationToken = default);

        Task<List<NotificationModel>> GetUnreadAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<List<NotificationModel>> GetRecentAsync(
            Guid userId,
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

        Task<List<NotificationListModel>> GetAllAsync(
            Guid userId,
            bool? unreadOnly = null,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            Guid notificationId,
            CancellationToken cancellationToken = default);
    }
}
