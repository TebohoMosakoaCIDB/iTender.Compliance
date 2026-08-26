namespace iTender.Compliance.Domain.Enums
{
    public enum CaseStatus
    {
        New,
        Assigned,
        WaitingForResponse = 3,

        Closed = 4,

        /// <summary>Letter generated and awaiting manager sign-off before it is sent.</summary>
        PendingApproval = 5,

        /// <summary>Contravention Notice issued, awaiting the 14-day response.</summary>
        ContraventionNoticeIssued = 6,

        /// <summary>Client has objected to an IL/CN; awaiting Manager decision.</summary>
        UnderManagerReview = 7,

        /// <summary>No response to the Contravention Notice; referred for enforcement.</summary>
        ReferredForEnforcement = 8
    }
}
