using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid? UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }

        public bool IsRead { get; set; }

        public string? Url { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
