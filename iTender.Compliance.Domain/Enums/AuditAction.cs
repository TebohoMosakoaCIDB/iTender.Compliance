namespace iTender.Compliance.Domain.Enums
{
    public enum AuditAction
    {
        Created = 1,
        Updated = 2,
        Deleted = 3,
        Imported = 4,

        Assigned = 5,
        Reassigned = 6,

        InstructionLetterSent = 7,
        ReminderLetterSent = 8,

        ResponseReceived = 9,

        CaseClosed = 10,

        SyncStarted = 11,
        SyncCompleted = 12,
        SyncFailed = 13,
        Error = 14,

        ContraventionNoticeSent = 15,
        ReferredToEnforcement = 16,
        ObjectionReceived = 17,
        ObjectionResolved = 18,
        ExtensionApproved = 19,
        CaseReopened = 20,
        ApprovalRequested = 21,
        ApprovalGranted = 22,
        ApprovalRejected = 23
    }
}
