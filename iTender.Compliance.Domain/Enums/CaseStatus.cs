namespace iTender.Compliance.Domain.Enums
{
    /// <summary>
    /// Case lifecycle status. Values map to the CIDB Compliance Monitor work baskets (BR-8.1).
    /// Names are kept compatible with existing code; new basket-statuses are appended so
    /// existing references (WaitingForResponse, ContraventionNoticeIssued, ReferredForEnforcement,
    /// PendingApproval, UnderManagerReview) continue to compile.
    /// </summary>
    public enum CaseStatus
    {
        // ------------------------------------------------------------
        // Existing values — DO NOT RENAME, they are referenced throughout
        // the codebase (repositories, services, reporting).
        // ------------------------------------------------------------

        /// <summary>Just created; not yet assigned or classified.</summary>
        New = 0,

        /// <summary>Assigned to a Compliance Officer.</summary>
        Assigned = 1,

        /// <summary>CO is actively working the case.</summary>
        InProgress = 2,

        /// <summary>Waiting for the client to respond to correspondence.</summary>
        WaitingForResponse = 3,

        /// <summary>Letter routed to Manager for approval (SigningHub).</summary>
        PendingApproval = 4,

        /// <summary>Client objection raised; Manager review in progress.</summary>
        UnderManagerReview = 5,

        /// <summary>Contravention Notice issued; 14-day corrective period running.</summary>
        ContraventionNoticeIssued = 6,

        /// <summary>Referred to Enforcement / AGSA.</summary>
        ReferredForEnforcement = 7,

        /// <summary>Case resolved and closed (terminal).</summary>
        Closed = 8,

        // ------------------------------------------------------------
        // New values added for BRS BR-8.1 baskets.
        // ------------------------------------------------------------

        /// <summary>
        /// BR-8.1 "Newly identified" basket.
        /// Tender ingested; construction classification pending CO review.
        /// Used before the case is assigned.
        /// </summary>
        NewlyIdentified = 9,

        /// <summary>
        /// BR-8.1 "Verification required" basket.
        /// Classification confirmed; compliance findings awaiting CO review.
        /// Set when the case has findings but no advertisement finding.
        /// </summary>
        VerificationRequired = 10,

        /// <summary>
        /// BR-8.1 "Not advertised" basket.
        /// Tender found on e-Tender/client site but not on i-Tender.
        /// Set when the case has an Advertisement finding.
        /// </summary>
        NotAdvertised = 11,

        /// <summary>
        /// BR-8.1 "Cleared/resolved" basket.
        /// Zero findings, or all findings resolved. Awaiting CO confirmation.
        /// </summary>
        Cleared = 12,
    }
}