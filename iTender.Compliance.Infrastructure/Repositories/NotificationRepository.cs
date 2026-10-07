using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class NotificationRepository : RepositoryBase, INotificationRepository
    {
        public NotificationRepository(IDbContextFactory<ComplianceDbContext> contextFactory)
            : base(contextFactory)
        {
        }

        public async Task<List<Notification>> GetUnreadAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Notifications
                .Where(x => x.UserId == userId && !x.IsRead)
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Notification>> GetRecentAsync(
            Guid userId,
            int count = 20,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Notifications
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedOn)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetUnreadCountAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Notifications
                .CountAsync(x =>
                    x.UserId == userId &&
                    !x.IsRead,
                    cancellationToken);
        }

        public async Task MarkAsReadAsync(
            Guid notificationId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var notification = await context.Notifications
                .FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken);

            if (notification == null)
                return;

            notification.IsRead = true;
            context.Notifications.Update(notification);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task MarkAllAsReadAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var notifications = await context.Notifications
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                context.Notifications.Update(notification);
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task AddAsync(
            Notification notification,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            await context.Notifications.AddAsync(notification, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Notification>> GetAllAsync(
            Guid userId,
            bool? unreadOnly = null,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var query = context.Notifications
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
            await using var context = await CreateContextAsync();

            var notification = await context.Notifications
                .FirstOrDefaultAsync(
                    x => x.Id == notificationId,
                    cancellationToken);

            if (notification == null)
                return;

            context.Notifications.Remove(notification);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<NotificationDetailModel?> GetByIdAsync(Guid id)
        {
            await using var context = await CreateContextAsync();

            return await context.Notifications
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

        public async Task<bool> ExistsUnreadAsync(Guid userId, string title, string? url, CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Notifications
                .AnyAsync(n =>
                    n.UserId == userId &&
                    !n.IsRead &&
                    n.Title == title &&
                    n.Url == url,
                    cancellationToken);
        }
    }
}