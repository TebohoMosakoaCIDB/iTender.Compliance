namespace iTender.Compliance.Domain.Enums
{
    public enum ClassificationStatus
    {
        Pending = 0,        // ingested, awaiting CO confirmation
        Confirmed = 1,      // CO confirmed as construction
        Rejected = 2,       // CO rejected (not construction)
        Overridden = 3,     // CO overrode the automated classification
        Processed = 4       // compliance pipeline has run
    }
}
