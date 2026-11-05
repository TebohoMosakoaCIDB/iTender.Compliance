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
        ReferralPending = 10,

        // Client response workflow
        ResponseReceived = 11,
        ResponseUnderReview = 12,
        AdditionalInformationRequested = 13,
        ResponseIncomplete = 14,
        AwaitingClientResubmission = 15,

        // Additional workflow stages
        AwaitingInstructionalResponse = 16,
        CorrectiveActionVerification = 17
    }
}
