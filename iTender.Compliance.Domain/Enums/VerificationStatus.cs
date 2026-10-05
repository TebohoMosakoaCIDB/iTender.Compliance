namespace iTender.Compliance.Domain.Enums
{
    public enum VerificationStatus
    {
        Pending,           // Verification has not yet been run
        AutomatedPassed,   // System verified compliance
        AutomatedFailed,   // System found non-compliance
        CoConfirmed,       // CO reviewed and confirmed the automated result
        CoOverridden,      // CO reviewed and overrode the automated result
        ManualPassed,      // CO manually verified and passed
        ManualFailed       // CO manually verified and failed
    }
}
