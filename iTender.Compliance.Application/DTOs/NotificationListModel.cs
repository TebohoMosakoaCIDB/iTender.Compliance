using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class NotificationListModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public DateTime CreatedOn { get; set; }

        public NotificationType Type { get; set; }
    }
}
