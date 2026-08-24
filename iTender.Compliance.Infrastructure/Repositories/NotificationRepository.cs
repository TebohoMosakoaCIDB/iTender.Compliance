using DocumentFormat.OpenXml.InkML;
using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class NotificationRepository : RepositoryBase, INotificationRepository
    {
        public NotificationRepository(ComplianceDbContext context)
            : base(context)
        {
        }

        public async Task<List<Notification>> GetUnreadAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await Context.Notifications
                .Where(x => x.UserId == userId && !x.IsRead)
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Notification>> GetRecentAsync(
            Guid userId,
            int count = 20,
            CancellationToken cancellationToken = default)
        {
            return await Context.Notifications
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedOn)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetUnreadCountAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await Context.Notifications
                .CountAsync(x =>
                    x.UserId == userId &&
                    !x.IsRead,
                    cancellationToken);
        }

        public async Task MarkAsReadAsync(
            Guid notificationId,
            CancellationToken cancellationToken = default)
        {
            var notification = await Context.Notifications
                .FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken);

            if (notification == null)
                return;

            notification.IsRead = true;
            Context.Notifications.Update(notification);
        }

        public async Task MarkAllAsReadAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var notifications = await Context.Notifications
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                Context.Notifications.Update(notification);
            }
        }

        public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            await Context.Notifications.AddAsync(notification);
        }

        public async Task<List<Notification>> GetAllAsync(
                Guid userId,
                bool? unreadOnly = null,
                CancellationToken cancellationToken = default)
        {
            var query = Context.Notifications
                .Where(x => x.UserId == userId);

            if (unreadOnly.HasValue)
            {
                query = query.Where(x => x.IsRead == !unreadOnly.Value);
            }

            return await query
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task DeleteAsync(
            Guid notificationId,
            CancellationToken cancellationToken = default)
        {
            var notification = await Context.Notifications
                .FirstOrDefaultAsync(
                    x => x.Id == notificationId,
                    cancellationToken);

            if (notification == null)
                return;

            Context.Notifications.Remove(notification);
        }

        public async Task<NotificationDetailModel?> GetByIdAsync(Guid id)
        {
            return await Context.Notifications
                .Where(x => x.Id == id)
                .Select(x => new NotificationDetailModel
                {
                    Id = x.Id,
                    Type = x.Type,
                    Title = x.Title,
                    Message = x.Message,
                    Url = x.Url,
                    IsRead = x.IsRead,
                    CreatedOn = x.CreatedOn
                })
                .FirstOrDefaultAsync();
        }
    }
}
