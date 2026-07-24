using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class TenderSyncSearchModel
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public SyncStatus? Status { get; set; }

        public bool? IsManual { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}
