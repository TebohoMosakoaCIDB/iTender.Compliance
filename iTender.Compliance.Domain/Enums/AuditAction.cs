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
        EscalatedToAGSA = 15,

        CaseAssigned = 16,
        InstructionalLetterSent = 17,
        ContraventionNoticeSent = 18,
        ExtensionGranted = 19,
        ObjectionRaised = 20,
        ExtensionRequested = 21,
        ErratumNoticeSent = 22
    }
}
