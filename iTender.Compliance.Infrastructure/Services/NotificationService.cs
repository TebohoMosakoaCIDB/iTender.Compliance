using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using iTender.Compliance.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace iTender.Compliance.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(
            INotificationRepository notificationRepository, IUnitOfWork unitOfWork, IHubContext<NotificationHub> hub)
        {
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
            _hub = hub;
        }

        public async Task NotifyAsync(
            CreateNotificationModel model,
            CancellationToken cancellationToken = default)
        {
            var notification = new Notification
            {
                UserId = model.UserId,
                Title = model.Title,
                Message = model.Message,
                Type = model.Type,
                Url = model.Url,
                IsRead = false,
                CreatedOn = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(
                notification,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _hub.Clients
                .User(model.UserId.ToString())
                .SendAsync("NotificationReceived");
        }

        public async Task<List<NotificationModel>> GetUnreadAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var notifications =
                await _notificationRepository.GetUnreadAsync(
                    userId,
                    cancellationToken);

            return notifications.Select(Map).ToList();
        }

        public async Task<List<NotificationModel>> GetRecentAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var notifications =
                await _notificationRepository.GetRecentAsync(
                    userId,
                    20,
                    cancellationToken);

            return notifications.Select(Map).ToList();
        }

        public Task<int> GetUnreadCountAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return _notificationRepository.GetUnreadCountAsync(
                userId,
                cancellationToken);
        }

        public async Task MarkAsReadAsync(
            Guid notificationId,
            CancellationToken cancellationToken = default)
        {
            await _notificationRepository.MarkAsReadAsync(
                notificationId,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task MarkAllAsReadAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            await _notificationRepository.MarkAllAsReadAsync(
                userId,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static NotificationModel Map(Notification notification)
        {
            return new NotificationModel
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                Url = notification.Url,
                IsRead = notification.IsRead,
                CreatedOn = notification.CreatedOn
            };
        }

        public async Task<List<NotificationListModel>> GetAllAsync(
            Guid userId,
            bool? unreadOnly = null,
            CancellationToken cancellationToken = default)
        {
            var notifications = await _notificationRepository.GetAllAsync(
                userId,
                unreadOnly,
                cancellationToken);

            return notifications
                .Select(x => new NotificationListModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Message = x.Message,
                    Url = x.Url,
                    IsRead = x.IsRead,
                    CreatedOn = x.CreatedOn,
                    Type = x.Type
                })
                .OrderByDescending(x => x.CreatedOn)
                .ToList();
        }

        public async Task DeleteAsync(
            Guid notificationId,
            CancellationToken cancellationToken = default)
        {
            await _notificationRepository.DeleteAsync(
                notificationId,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
