namespace iTender.Compliance.Domain.Enums
{
    public enum SigningRequestStatus
    {
        Draft = 1,

        Uploaded = 2,

        PendingSignature = 3,

        Viewed = 4,

        Signed = 5,

        Declined = 6,

        Expired = 7,

        Cancelled = 8,

        Failed = 9
    }
}
