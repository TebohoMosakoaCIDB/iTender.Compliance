using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class TenderSyncLogModel
    {
        public DateTime Date { get; set; }

        public SyncLogType Type { get; set; }

        public SyncLogLevel Level { get; set; }

        public string? TenderNumber { get; set; }

        public string Title { get; set; } = "";

        public string Message { get; set; } = "";
    }
}
