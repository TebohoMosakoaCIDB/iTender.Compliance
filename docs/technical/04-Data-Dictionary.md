# 04 – Data Dictionary

## Document control

| Item | Value |
|---|---|
| System | iTender Compliance |
| Document | Data Dictionary |
| Version | 1.0 – source-derived draft |
| ORM context | `ComplianceDbContext` |
| Database provider | SQL Server configured by the Client startup |
| Source baseline | Repository default branch at preparation time |

> **Interpretation note:** This dictionary is derived from the C# entity classes and the EF Core configuration files. “Nullable” reflects the CLR declaration (for example, `string?` or `Guid?`) and not a live inspection of SQL Server. Entity Framework Core nullable-reference-type settings, explicit configuration and migrations determine the final database nullability. Confirm physical column types, lengths, defaults and names against the deployed database before using this as a production schema extract.

## 1. Conventions

- Most domain entities inherit from `BaseEntity`.
- `Id` is a GUID primary key for entities inheriting from `BaseEntity`.
- CLR enum properties are normally persisted as integer values unless a value converter is configured.
- Navigation properties describe relationships and are not separate scalar columns.
- `virtual` navigation properties are omitted from the field tables below.
- SQL Server mappings are typically: `Guid → uniqueidentifier`, `string → nvarchar`, `int/enum → int`, `bool → bit`, `DateTime → datetime2`, `decimal → decimal`, `double → float`. Confirm precision and exact column types from migrations.
- “Required by declaration” means a non-nullable CLR value or non-nullable reference declaration. It does not replace a database constraint review.

## 2. Shared base fields

Source: `iTender.Compliance.Domain/Entities/BaseEntity.cs`.

| Field | CLR type | Nullable | Default / behaviour | Description |
|---|---|---:|---|---|
| `Id` | `Guid` | No | No explicit initializer | Primary key. |
| `CreatedOn` | `DateTime` | No | `DateTime.UtcNow` | Timestamp when the entity instance is created. |
| `CreatedBy` | `Guid?` | Yes | `null` | Optional identifier of the creating user. |
| `ModifiedOn` | `DateTime?` | Yes | Initialized to `DateTime.UtcNow` in the base class | Modification timestamp; note the nullable declaration and non-null initializer. |
| `ModifiedBy` | `Guid?` | Yes | `null` | Optional identifier of the user who last modified the entity. |

The base class does not itself enforce automatic modification timestamp updates. Confirm where save-time auditing is implemented before relying on `ModifiedOn` and `ModifiedBy`.

## 3. Tender intake and synchronization

### 3.1 Tender

Entity: `Tender` · Configured table: `Tenders`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `TenderNumber` | `string` | No | Empty string; max 500; unique index | Tender reference/number. |
| `Title` | `string` | No | Empty string; max 5,000 | Tender title. |
| `NormalizedTitle` | `string` | No | Empty string | Normalized title used for matching/searching. |
| `Description` | `string?` | Yes | — | Tender description. |
| `EmployerName` | `string` | No | Empty string | Procuring employer name. |
| `ContactName` | `string?` | Yes | — | Tender contact name. |
| `ContactEmail` | `string?` | Yes | — | Tender contact email. |
| `ContactNumber` | `string?` | Yes | — | Tender contact telephone number. |
| `AdvertisedDate` | `DateTime` | No | — | Date/time tender was advertised. |
| `ClosingDate` | `DateTime` | No | — | Tender closing date/time. |
| `Source` | `string` | No | Empty string | Origin/source system for the tender. |
| `TenderUrl` | `string` | No | Empty string; max 4,000 | Link to tender information. |
| `ExternalId` | `string?` | Yes | — | Source-system identifier. |
| `Ocid` | `string?` | Yes | — | Open Contracting ID, where supplied. |
| `ProcurementCategory` | `string?` | Yes | — | Procurement category from source data. |
| `Province` | `string?` | Yes | — | Province associated with the tender. |
| `DeliveryLocation` | `string?` | Yes | — | Delivery/performance location. |
| `ProcurementMethod` | `string?` | Yes | — | Procurement method. |
| `ProcurementMethodDetails` | `string?` | Yes | — | Additional procurement-method details. |
| `TenderStatus` | `string?` | Yes | — | Status supplied by source. |
| `SpecialConditions` | `string?` | Yes | — | Tender special conditions. |
| `SourceDocumentUrl` | `string?` | Yes | — | Link to source document. |
| `TenderSyncId` | `Guid` | No | FK to `TenderSync` | Sync run associated with the tender. |
| `IsConstruction` | `bool` | No | CLR default `false` | Whether tender is classified as construction. |
| `ClassOfWorks` | `string?` | Yes | — | Determined class of works. |
| `ClassOfWorksSource` | `ClassOfWorksSource` | No | `NotSpecified` | Origin of the class-of-works value. |
| `ClaimedClassOfWorks` | `string?` | Yes | — | Class of works claimed in tender/source information. |
| `AwardedDate` | `DateTime?` | Yes | — | Award date. |
| `AwardValue` | `decimal?` | Yes | — | Award value. |
| `WinningContractor` | `string?` | Yes | — | Winning contractor name. |
| `IsRegisteredOnRoP` | `bool` | No | CLR default `false` | Whether the tender/award is registered on the Register of Projects. |
| `RoPRegistrationDate` | `DateTime?` | Yes | — | RoP registration date. |
| `ClassificationStatus` | `ClassificationStatus` | No | `Pending` | Classification review state. |
| `ClassificationReviewedByUserId` | `Guid?` | Yes | — | User identifier of the reviewer. |
| `ClassificationReviewedOn` | `DateTime?` | Yes | — | Classification review timestamp. |
| `ClassificationReviewNotes` | `string?` | Yes | — | Reason/notes for classification review or override. |
| `ClassificationOverridden` | `bool` | No | CLR default `false` | Whether a compliance officer overrode automated classification. |
| `IsAdvertised` | `bool` | No | CLR default `false` | Whether the tender was advertised on the CIDB i-Tender portal. |
| `NormalizedEmployerName` | `string` | No | Empty string | Canonical employer name used for client grouping. |

### 3.2 TenderSync

Entity: `TenderSync`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `StartedOn` | `DateTime` | No | — | Sync start timestamp. |
| `CompletedOn` | `DateTime?` | Yes | — | Sync completion timestamp. |
| `Status` | `SyncStatus` | No | Enum default | Overall sync state. |
| `IsManual` | `bool` | No | `false` | Whether a user manually initiated the run. |
| `StartedByUserId` | `Guid?` | Yes | — | Initiating user identifier. |
| `TotalRetrieved` | `int` | No | `0` | Number of tenders retrieved. |
| `TotalCompliant` | `int` | No | `0` | Number classified as compliant. |
| `TotalNonCompliant` | `int` | No | `0` | Number classified as non-compliant. |
| `CasesCreated` | `int` | No | `0` | Number of cases created during the run. |
| `ErrorCount` | `int` | No | `0` | Number of errors recorded. |
| `Notes` | `string?` | Yes | — | Additional sync notes. |

### 3.3 TenderSyncLog

Entity: `TenderSyncLog`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `TenderSyncId` | `Guid` | No | FK to `TenderSync` | Parent sync run. |
| `Type` | `SyncLogType` | No | Enum default | Log event category. |
| `Level` | `SyncLogLevel` | No | Enum default | Severity level. |
| `Title` | `string` | No | Empty string | Short log title. |
| `Message` | `string` | No | Empty string | Log details/message. |
| `TenderNumber` | `string?` | Yes | — | Related tender number, when applicable. |

## 4. Compliance case management

### 4.1 ComplianceCase

Entity: `ComplianceCase` · Configured table: `ComplianceCases`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `TenderId` | `Guid` | No | Unique FK to `Tender` | Tender associated with this case; one-to-one relationship. |
| `AgentId` | `Guid?` | Yes | FK to `Agent`; set null on agent deletion | Assigned compliance officer/agent. |
| `Status` | `CaseStatus` | No | Enum default | Current case workflow status. |
| `Outcome` | `ComplianceOutcome?` | Yes | — | Compliance outcome, when determined. |
| `Priority` | `CasePriority` | No | Enum default | Case priority. |
| `Type` | `LetterType` | No | Enum default | Case/letter classification represented by this field. |
| `ClosedDate` | `DateTime?` | Yes | — | Case closure timestamp/date. |
| `Comments` | `string?` | Yes | — | Case-level comments. |
| `AssignedOn` | `DateTime?` | Yes | — | Assignment timestamp. |
| `ClosureReason` | `CaseClosureReason?` | Yes | — | Reason for closing the case. |
| `Track` | `TenderComplianceTrack` | No | `Open` | Compliance track, such as open/closed tender workflow. |
| `ExtensionDays` | `int?` | Yes | — | Number of extension days granted. |
| `ExtendedDueOn` | `DateTime?` | Yes | — | Revised due date after extension. |

### 4.2 CaseLetter

Entity: `CaseLetter` · Configured table: `CaseLetters`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `ComplianceCaseId` | `Guid` | No | FK to `ComplianceCase` | Parent compliance case. |
| `Type` | `LetterType` | No | Enum default | Letter type. |
| `LetterNumber` | `int` | No | `0` | Letter sequence/number. |
| `ClientGroupId` | `Guid?` | Yes | Optional group association | Client group for consolidated correspondence. |
| `RecipientName` | `string` | No | Empty string | Recipient name. |
| `RecipientEmail` | `string` | No | Empty string; max 250 configured | Recipient email address. |
| `FileName` | `string` | No | Empty string; max 250 configured | Generated letter filename. |
| `FilePath` | `string` | No | Empty string; max 500 configured | Stored document path/reference. |
| `EmailSent` | `bool` | No | `false` | Whether the letter email was sent. |
| `SentOn` | `DateTime` | No | — | Letter sent timestamp. |
| `ResponseDueOn` | `DateTime` | No | — | Recipient response deadline. |
| `RespondedOn` | `DateTime?` | Yes | — | Date/time response received. |
| `Accepted` | `bool?` | Yes | — | Whether the response was accepted, when recorded. |
| `ResponseComments` | `string?` | Yes | — | Response comments. |
| `ComplianceFindingId` | `Guid?` | Yes | Optional finding reference | Finding addressed by this letter. |

### 4.3 CaseNote

Entity: `CaseNote`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `ComplianceCaseId` | `Guid` | No | FK to case | Parent case. |
| `Comment` | `string` | No | Empty string | Note/comment content. |
| `CreatedByUserId` | `Guid?` | Yes | — | User who entered the note. |

### 4.4 ComplianceFinding

Entity: `ComplianceFinding`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `ComplianceCaseId` | `Guid` | No | FK to case | Parent case. |
| `Stream` | `ComplianceStream` | No | Enum default | Compliance stream/category. |
| `FindingType` | `ComplianceFindingType` | No | Enum default | Finding classification. |
| `VerificationStatus` | `VerificationStatus` | No | `Pending` | Verification state. |
| `Description` | `string` | No | Empty string | Finding details. |
| `RegulatoryReference` | `string` | No | Empty string | Applicable regulatory reference. |
| `IdentifiedAt` | `DateTime` | No | — | Finding identification timestamp. |
| `IsResolved` | `bool` | No | `false` | Whether the finding is resolved. |
| `ResolvedOn` | `DateTime?` | Yes | — | Resolution timestamp. |
| `TenderStatusAtCheck` | `TenderStatus` | No | Enum default | Tender status when the finding was checked. |

### 4.5 ComplianceAction

Entity: `ComplianceAction`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `ComplianceCaseId` | `Guid` | No | FK to case | Parent case. |
| `ActionType` | `ComplianceActionType` | No | Enum default | Action category. |
| `Status` | `ComplianceActionStatus` | No | Enum default | Action workflow state. |
| `ActionDate` | `DateTime` | No | — | Date/time action was recorded or initiated. |
| `ResponseDueDate` | `DateTime?` | Yes | — | Due date for the action/response. |
| `CompletedDate` | `DateTime?` | Yes | — | Completion date. |
| `Comments` | `string?` | Yes | — | Additional action notes. |

### 4.6 CaseObjection

Entity: `CaseObjection` · Configured table: `CaseObjections`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `ComplianceCaseId` | `Guid` | No | FK to case | Case being challenged. |
| `CaseLetterId` | `Guid` | No | FK to letter | Letter to which objection relates. |
| `ReceivedOn` | `DateTime` | No | — | Objection received timestamp. |
| `Reason` | `string` | No | Empty string | Objection reason. |
| `Status` | `ObjectionStatus` | No | Enum default | Objection workflow state. |
| `ReviewedByAgentId` | `Guid?` | Yes | — | Agent who reviewed the objection. |
| `ReviewedOn` | `DateTime?` | Yes | — | Review timestamp. |
| `Decision` | `ObjectionDecision?` | Yes | — | Decision reached, when available. |
| `ManagerNotes` | `string?` | Yes | — | Manager review notes. |

### 4.7 AGSAReferral

Entity: `AGSAReferral` · Configured table: `AGSAReferrals`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `ComplianceCaseId` | `Guid` | No | Unique FK to case | Case referred; one referral per case enforced by unique index. |
| `ReferralNumber` | `string` | No | Empty string; max 50 configured | Referral reference number. |
| `ReferralDate` | `DateTime` | No | — | Referral date. |
| `ReferredByUserId` | `Guid?` | Yes | — | User who made the referral. |
| `Reason` | `string` | No | Empty string | Referral reason. |
| `Description` | `string?` | Yes | — | Additional referral description. |
| `Status` | `EnforcementReferralStatus` | No | Enum default | Referral status. |
| `FileName` | `string?` | Yes | — | Referral document filename. |
| `FilePath` | `string?` | Yes | — | Referral document path/reference. |
| `AgsaResponseDate` | `DateTime?` | Yes | — | Date AGSA response was received. |
| `AgsaResponse` | `string?` | Yes | — | AGSA response details. |

## 5. Workforce, client grouping and duplicate review

### 5.1 Agent

Entity: `Agent` · Configured table: `Agents`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `UserId` | `Guid` | No | Unique index | Associated application user identifier. |
| `EmployeeNumber` | `string` | No | Empty string; max 100 configured | CIDB employee number. |
| `FullName` | `string` | No | Empty string | Agent display name. |
| `Department` | `string` | No | Empty string | Department. |
| `Email` | `string` | No | Empty string | Work email. |
| `PhoneNumber` | `string` | No | Empty string | Contact telephone number. |
| `IsActive` | `bool` | No | `true` | Whether the agent is active. |
| `IsManager` | `bool` | No | `false` | Whether the agent has manager responsibilities. |
| `AutoAssignEnabled` | `bool` | No | `true` | Whether automatic assignment can select this agent. |
| `MaximumOpenGroups` | `int` | No | `0` | Maximum open client groups for allocation. |
| `DisplayOrder` | `int` | No | `0` | Display/sort order. |
| `Notes` | `string?` | Yes | — | Administrative notes. |
| `Level` | `AgentLevel` | No | Enum default | Agent level. |
| `JobTitle` | `string?` | Yes | — | Job title. |
| `HeaderImagePath` | `string?` | Yes | — | Header image path/reference for correspondence. |
| `SignatureImagePath` | `string?` | Yes | — | Signature image path/reference. |
| `FooterText` | `string?` | Yes | — | Correspondence footer text. |

### 5.2 ClientGroup

Entity: `ClientGroup` · Configured table: `ClientGroups`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `NormalizedEmployerName` | `string` | No | Empty string; indexed | Canonical normalized employer name. |
| `EmployerName` | `string` | No | Empty string | Display employer name. |
| `AgentId` | `Guid?` | Yes | Optional agent relationship | Assigned agent. |
| `Status` | `ClientGroupStatus` | No | Enum default; indexed | Client-group workflow state. |
| `ContactName` | `string?` | Yes | — | Primary consolidated-letter contact. |
| `ContactEmail` | `string?` | Yes | — | Primary recipient email. |
| `CcEmails` | `string?` | Yes | Semicolon-separated addresses per entity comment | CC recipients. |
| `ConsolidatedLetterId` | `Guid?` | Yes | Optional letter reference | Consolidated letter generated for the group. |
| `IssuedOn` | `DateTime?` | Yes | — | Consolidated letter issue timestamp. |
| `AnnexureFileName` | `string?` | Yes | — | Annexure filename. |
| `AnnexureFilePath` | `string?` | Yes | — | Annexure storage path/reference. |

### 5.3 ClientGroupCase

Join entity: `ClientGroupCase` · Configured table: `ClientGroupCases`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `Id` | `Guid` | No | Primary key | Join-row identifier. |
| `ClientGroupId` | `Guid` | No | FK to group | Parent client group. |
| `ComplianceCaseId` | `Guid` | No | FK to case | Associated compliance case. |
| `AddedOn` | `DateTime` | No | `DateTime.UtcNow` | Date/time case was added to group. |

A unique composite index on `ClientGroupId, ComplianceCaseId` prevents duplicate membership. An additional index exists on `ComplianceCaseId`.

### 5.4 DuplicateGroup

Entity: `DuplicateGroup` · Configured table: `DuplicateGroups`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `TenderSyncId` | `Guid` | No | FK to sync run | Sync run that identified the duplicate candidates. |
| `Status` | `DuplicateStatus` | No | Enum default; indexed | Duplicate-review state. |
| `ConfirmedByUserId` | `Guid?` | Yes | — | User who confirmed the review outcome. |
| `ConfirmedOn` | `DateTime?` | Yes | — | Confirmation timestamp. |
| `CoComment` | `string?` | Yes | — | Compliance officer comment. |

### 5.5 DuplicateCandidate

Entity: `DuplicateCandidate` · Configured table: `DuplicateCandidates`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `DuplicateGroupId` | `Guid` | No | FK to duplicate group; indexed | Parent duplicate-review group. |
| `TenderId` | `Guid` | No | FK to tender; indexed | Candidate tender. |
| `MatchType` | `DuplicateMatchType` | No | Enum default | Matching method/category. |
| `ConfidenceScore` | `double` | No | `0` | Matching confidence score. |
| `MatchedOn` | `string?` | Yes | — | Field(s) or evidence used for matching. |

## 6. Correspondence, signing and system operations

### 6.1 SigningRequest

Entity: `SigningRequest` · Configured table: `SigningRequests`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `CaseLetterId` | `Guid` | No | FK to case letter; one-to-one | Letter submitted for signing. |
| `Status` | `SigningRequestStatus` | No | Enum default | Signing workflow status. |
| `DocumentId` | `string?` | Yes | — | SigningHub document identifier. |
| `PackageId` | `int?` | Yes | — | SigningHub package identifier. |
| `OriginalDocumentPath` | `string` | No | Empty string | Original document path/reference. |
| `SignedDocumentPath` | `string?` | Yes | — | Signed document path/reference. |
| `SignerId` | `Guid?` | Yes | — | Signer identifier. |
| `SignerName` | `string?` | Yes | — | Signer name. |
| `SignerEmail` | `string?` | Yes | — | Signer email. |
| `SentOn` | `DateTime?` | Yes | — | Date/time sent for signing. |
| `SignedOn` | `DateTime?` | Yes | — | Date/time signed. |
| `FailureReason` | `string?` | Yes | — | Signing failure details. |
| `FileName` | `string` | No | Empty string | Document filename. |

### 6.2 CorrespondenceTemplateModel

Entity: `CorrespondenceTemplateModel` · DbSet: `CorrespondenceTemplates`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `Name` | `string` | No | Empty string | Template name. |
| `Type` | `CorrespondenceTemplateType` | No | Enum default | Template type. |
| `Status` | `CorrespondenceTemplateStatus` | No | Enum default | Template approval/lifecycle status. |
| `Version` | `int` | No | `1` | Template version. |
| `Subject` | `string` | No | Empty string | Email/letter subject template. |
| `Body` | `string` | No | Empty string | Template body/content. |
| `IsActive` | `bool` | No | `false` | Whether template is active. |
| `CreatedOn` | `DateTime` | No | No initializer in this class | Template creation timestamp; hides/redeclares the base member by name and requires model review. |
| `UpdatedOn` | `DateTime?` | Yes | — | Last update timestamp. |
| `CreatedBy` | `string?` | Yes | — | Creator identifier/name as string; this hides the inherited `Guid?` member by name and requires explicit EF model verification. |
| `UpdatedBy` | `string?` | Yes | — | Updater identifier/name. |
| `ApprovedOn` | `DateTime?` | Yes | — | Approval timestamp. |
| `ApprovedBy` | `string?` | Yes | — | Approver identifier/name. |
| `ApprovalComments` | `string?` | Yes | — | Approval comments. |

**Model review required:** This entity redeclares `CreatedOn` and `CreatedBy` names already present on `BaseEntity`, but with different declarations/types. Confirm the EF Core model snapshot and resulting physical columns to establish whether these are mapped as hidden members, ignored, or otherwise configured.

### 6.3 AuditLog

Entity: `AuditLog` · Configured table: `AuditLogs`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `Action` | `AuditAction` | No | Enum default | Audit action category. |
| `Entity` | `AuditEntity` | No | Enum default | Type/category of entity affected. |
| `EntityId` | `Guid` | No | — | Identifier of the affected record; generic reference, not a FK to every entity. |
| `Description` | `string` | No | Empty string | Human-readable audit description. |

### 6.4 Notification

Entity: `Notification` · DbSet: `Notifications`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `UserId` | `Guid?` | Yes | — | Optional target user; null may represent a general notification depending on service logic. |
| `Title` | `string` | No | Empty string | Notification title. |
| `Message` | `string` | No | Empty string | Notification body. |
| `Type` | `NotificationType` | No | Enum default | Notification category. |
| `IsRead` | `bool` | No | `false` | Whether notification has been read. |
| `Url` | `string?` | Yes | — | Optional navigation target. |
| `CreatedOn` | `DateTime` | No | `DateTime.UtcNow` | Notification creation timestamp; redeclares the base property by name and requires model verification. |

### 6.5 SystemSetting

Entity: `SystemSetting` · DbSet: `SystemSettings`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `ResponseDueHours` | `int` | No | `48` | Legacy/general response period in hours. |
| `ReminderDelayHours` | `int` | No | `48` | Reminder delay in hours. |
| `SynchronizationIntervalHours` | `int` | No | `12` | Tender synchronization interval. |
| `MaximumReminders` | `int` | No | `3` | Maximum reminder count. |
| `DefaultPageSize` | `int` | No | `20` | Default page size. |
| `EnableAutomaticReminders` | `bool` | No | `true` | Enables automatic reminders. |
| `ReminderAfterHours` | `int` | No | `48` | Reminder threshold in hours. |
| `ReminderCheckIntervalMinutes` | `int` | No | `60` | Reminder/SLA processing interval. |
| `AutoAssignmentEnabled` | `bool` | No | `false` | Enables automatic assignment. |
| `DistributionMethod` | `CaseDistributionMethod` | No | Enum default | Case allocation strategy. |
| `RoundRobinCursor` | `int` | No | `0` | Persisted round-robin position/cursor. |
| `DefaultMaximumOpenCases` | `int` | No | `0` | Default workload cap. |
| `OpenTenderResponseHours` | `int` | No | `48`; obsolete | Legacy open-tender response setting; source marks it superseded. |
| `ClosedTenderResponseDays` | `int` | No | `14` | Closed-tender response period in days. |
| `ContraventionNoticeResponseDays` | `int` | No | `14` | Contravention Notice response period in working days per source comment. |
| `InstructionLetterResponseWorkingDays` | `int` | No | `2` | Instruction Letter response period in working days. |
| `ReminderAfterWorkingDays` | `int` | No | `7` | Reminder point in working days. |
| `AgsaReferralDeadlineDays` | `int` | No | `30` | Case allocation-to-resolution/referral deadline in calendar days. |
| `RopCheckAfterDays` | `int` | No | `90` | Delay after tender close before RoP check. |
| `RopRegistrationGraceDays` | `int` | No | `21` | Additional grace period after RoP check threshold. |
| `AgsaReferralEmail` | `string` | No | Default email in source | Enforcement referral recipient. |
| `EnforcementUnitEmail` | `string` | No | Default email in source | Enforcement unit CC recipient. |
| `RequireManagerApproval` | `bool` | No | `true` | Whether manager sign-off is required before sending letters. |
| `InstructionLetterReminderAfterWorkingDays` | `int` | No | `1` | Reminder point for Instruction Letters. |
| `SendMonthlyReportAutomatically` | `bool` | No | `false` | Enables monthly report email. |
| `MonthlyReportRecipientEmail` | `string?` | Yes | — | Monthly report recipient. |
| `MonthlyReportHourUtc` | `int` | No | `6` | UTC hour for monthly report job. |
| `LastMonthlyReportSent` | `string?` | Yes | Expected format yyyy-MM | Last monthly reporting period sent. |
| `SendQuarterlyReportAutomatically` | `bool` | No | `false` | Enables quarterly report email. |
| `QuarterlyReportRecipientEmail` | `string?` | Yes | — | Quarterly report recipient. |
| `LastQuarterlyReportSent` | `string?` | Yes | — | Last quarterly reporting period sent. |
| `SlaWarningDays` | `int` | No | `25` | Case age at which SLA warning is issued. |

### 6.6 PublicHoliday

Entity: `PublicHoliday` · Configured table: `PublicHolidays`

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `Date` | `DateOnly` | No | Unique index | Public holiday date. |
| `Name` | `string` | No | Empty string; max 100 configured | Holiday name. |
| `IsObservedShift` | `bool` | No | `false` | Indicates an observed-shift row where a Sunday holiday is observed on Monday. |

## 7. ASP.NET Core Identity data

The context derives from `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`. The custom user entity is `ApplicationUser : IdentityUser<Guid>`.

### 7.1 ApplicationUser custom fields

| Field | CLR type | Nullable | Default / notes | Description |
|---|---|---:|---|---|
| `FirstName` | `string` | No | Empty string | User first name. |
| `LastName` | `string` | No | Empty string | User surname. |
| `FullName` | `string` (computed) | No | Concatenation of first and last name | Read-only convenience property; not assumed to be a persisted column. |
| `IsActive` | `bool` | No | `true` | Application-level active flag. |
| `CreatedOn` | `DateTime` | No | `DateTime.UtcNow` | User creation timestamp. |
| `LastLoginOn` | `DateTime?` | Yes | — | Last successful login timestamp. |

### 7.2 Standard Identity fields

The base `IdentityUser<Guid>` contributes the standard Identity user fields, including:

- `Id`, `UserName`, `NormalizedUserName`, `Email`, `NormalizedEmail`
- `EmailConfirmed`, `PasswordHash`, `SecurityStamp`, `ConcurrencyStamp`
- `PhoneNumber`, `PhoneNumberConfirmed`
- `TwoFactorEnabled`, `LockoutEnd`, `LockoutEnabled`, `AccessFailedCount`

Identity also manages role, user-role, user-claim, role-claim, external-login and token records. These tables should be documented from the actual Identity migration/model snapshot if a full physical column-by-column Identity schema is required.

## 8. Enum-backed columns

The following entity properties are enum-backed and should be treated as persisted application data:

| Enum | Used by |
|---|---|
| `ClassOfWorksSource` | `Tender.ClassOfWorksSource` |
| `ClassificationStatus` | `Tender.ClassificationStatus` |
| `SyncStatus` | `TenderSync.Status` |
| `SyncLogType` | `TenderSyncLog.Type` |
| `SyncLogLevel` | `TenderSyncLog.Level` |
| `CaseStatus` | `ComplianceCase.Status` |
| `ComplianceOutcome` | `ComplianceCase.Outcome` |
| `CasePriority` | `ComplianceCase.Priority` |
| `LetterType` | `ComplianceCase.Type`, `CaseLetter.Type` |
| `CaseClosureReason` | `ComplianceCase.ClosureReason` |
| `TenderComplianceTrack` | `ComplianceCase.Track` |
| `ComplianceStream` | `ComplianceFinding.Stream` |
| `ComplianceFindingType` | `ComplianceFinding.FindingType` |
| `VerificationStatus` | `ComplianceFinding.VerificationStatus` |
| `TenderStatus` | `ComplianceFinding.TenderStatusAtCheck` |
| `ComplianceActionType` | `ComplianceAction.ActionType` |
| `ComplianceActionStatus` | `ComplianceAction.Status` |
| `ObjectionStatus` | `CaseObjection.Status` |
| `ObjectionDecision` | `CaseObjection.Decision` |
| `EnforcementReferralStatus` | `AGSAReferral.Status` |
| `AgentLevel` | `Agent.Level` |
| `ClientGroupStatus` | `ClientGroup.Status` |
| `DuplicateStatus` | `DuplicateGroup.Status` |
| `DuplicateMatchType` | `DuplicateCandidate.MatchType` |
| `CorrespondenceTemplateType` | `CorrespondenceTemplateModel.Type` |
| `CorrespondenceTemplateStatus` | `CorrespondenceTemplateModel.Status` |
| `SigningRequestStatus` | `SigningRequest.Status` |
| `AuditAction` | `AuditLog.Action` |
| `AuditEntity` | `AuditLog.Entity` |
| `NotificationType` | `Notification.Type` |
| `CaseDistributionMethod` | `SystemSetting.DistributionMethod` |

**Enum governance:** Do not reorder or renumber enum members after values have been persisted. Add explicit numeric assignments and migration/data-conversion steps where a persisted enum contract needs to change. A detailed value/meaning catalogue should be maintained against the source enum definitions.

## 9. Key and relationship field reference

| FK field | Dependent entity | Principal entity | Notes |
|---|---|---|---|
| `TenderSyncId` | `Tender` | `TenderSync` | Required. |
| `TenderSyncId` | `TenderSyncLog` | `TenderSync` | Required. |
| `TenderId` | `ComplianceCase` | `Tender` | Required; one-to-one unique relationship. |
| `AgentId` | `ComplianceCase` | `Agent` | Optional; set null on agent deletion. |
| `ComplianceCaseId` | `CaseLetter` | `ComplianceCase` | Required. |
| `ComplianceCaseId` | `CaseNote` | `ComplianceCase` | Required. |
| `ComplianceCaseId` | `ComplianceFinding` | `ComplianceCase` | Required. |
| `ComplianceCaseId` | `ComplianceAction` | `ComplianceCase` | Required. |
| `ComplianceCaseId` | `CaseObjection` | `ComplianceCase` | Required. |
| `ComplianceCaseId` | `AGSAReferral` | `ComplianceCase` | Required; unique index on FK. |
| `CaseLetterId` | `SigningRequest` | `CaseLetter` | Required; one-to-one. |
| `CaseLetterId` | `CaseObjection` | `CaseLetter` | Required. |
| `ClientGroupId` | `ClientGroupCase` | `ClientGroup` | Required. |
| `ComplianceCaseId` | `ClientGroupCase` | `ComplianceCase` | Required; composite unique membership index. |
| `AgentId` | `ClientGroup` | `Agent` | Optional. |
| `ConsolidatedLetterId` | `ClientGroup` | `CaseLetter` | Optional reference; verify configured FK/delete behaviour. |
| `DuplicateGroupId` | `DuplicateCandidate` | `DuplicateGroup` | Required. |
| `TenderId` | `DuplicateCandidate` | `Tender` | Required. |
| `TenderSyncId` | `DuplicateGroup` | `TenderSync` | Required. |

Some user identifier fields (for example `CreatedBy`, `StartedByUserId`, `ConfirmedByUserId`, `ReferredByUserId`, `ReviewedByAgentId`, `ClassificationReviewedByUserId`) are scalar GUIDs. Do not assume they are enforced foreign keys to Identity users or agents unless the EF configuration explicitly maps them.

## 10. Fields requiring special attention

1. **Hidden base members:** `CorrespondenceTemplateModel` redeclares `CreatedOn` and `CreatedBy`; `Notification` redeclares `CreatedOn`. Confirm the EF model and migration output to determine the actual persisted properties/columns.
2. **CaseLetter finding link:** `ComplianceFindingId` is nullable. Verify whether a relationship is configured or whether it is currently an informational scalar reference.
3. **ClientGroup consolidated letter:** `ConsolidatedLetterId` is nullable; confirm the configured relationship and deletion behaviour.
4. **SystemSetting cardinality:** The entity contains settings but the class alone does not enforce a single-row configuration. Confirm service logic and database constraints.
5. **String lengths:** Explicit maximum lengths exist for selected properties. Review other strings for appropriate limits, especially log messages, template bodies, comments and source payload-derived fields.
6. **Decimals:** Confirm precision/scale for `Tender.AwardValue` in the generated migration and deployed schema.
7. **Date/time conventions:** Many fields use `DateTime`; verify that stored values are consistently UTC and that date-only business rules are not affected by time-zone conversions.
8. **File references:** File path fields store references; verify actual binary storage, authorization, backup and retention separately.

## 11. Source files

- `iTender.Compliance.Infrastructure/Data/ComplianceDbContext.cs`
- `iTender.Compliance.Infrastructure/Data/Configurations/`
- `iTender.Compliance.Infrastructure/Identity/ApplicationUser.cs`
- `iTender.Compliance.Infrastructure/Migrations/ComplianceDbContextModelSnapshot.cs`
- `iTender.Compliance.Domain/Entities/BaseEntity.cs`
- `iTender.Compliance.Domain/Entities/`
- `iTender.Compliance.Domain/Enums/`
- `iTender.Compliance.Client/Program.cs`

## 12. Outstanding validation

Before approving this as the final production data dictionary:

- Export the actual SQL Server schema, including column SQL types, lengths, precision/scale, nullability, defaults, computed columns, indexes and constraints.
- Compare the deployed schema with the latest `ComplianceDbContextModelSnapshot` and applied migrations.
- Resolve the hidden-member mapping concerns described above.
- Confirm all enum numeric values and document their business meanings.
- Confirm actual table names for entities without explicit `ToTable` configuration.
- Confirm FK coverage for user IDs and optional scalar references.
- Confirm the intended uniqueness and cardinality of settings, referrals, tender numbers and group membership.

---

**Document maintenance:** Update this dictionary whenever an entity, property, enum, EF Core configuration, migration or database provider changes.
