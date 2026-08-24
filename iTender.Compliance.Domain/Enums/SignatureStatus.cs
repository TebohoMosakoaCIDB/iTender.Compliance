namespace iTender.Compliance.Domain.Enums
{
    public enum SignatureStatus
    {
        Pending = 1,

        AwaitingSignature = 2,

        Viewed = 3,

        InProgress = 4,

        Completed = 5,

        Rejected = 6,

        Cancelled = 7,

        Expired = 8,

        Failed = 9
    }
}
