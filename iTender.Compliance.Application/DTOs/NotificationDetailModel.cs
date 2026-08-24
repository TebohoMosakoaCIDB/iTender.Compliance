using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class NotificationDetailModel
    {
        public Guid Id { get; set; }

        public NotificationType Type { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string? Url { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ReadOn { get; set; }
    }
}
