namespace iTender.Compliance.Domain.Enums
{
    public enum WorkBasket
    {
        NewlyIdentified = 1,
        VerificationRequired = 2,
        NotAdvertised = 3,
        DueForInstructionLetter = 4,
        DueForContraventionNotice = 5,
        CorrectiveAction = 6,
        NonCompliant = 7,
        Referral = 8,
        ClearedResolved = 9,
        ReferralPending = 10
    }
}
