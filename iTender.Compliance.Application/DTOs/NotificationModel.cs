using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class NotificationModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }

        public bool IsRead { get; set; }

        public string? Url { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
