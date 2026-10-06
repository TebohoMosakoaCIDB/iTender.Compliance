using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Domain.Utilities;
using Microsoft.Extensions.Logging;

namespace iTender.Compliance.Infrastructure.Services
{
    public class ClientGroupService : IClientGroupService
    {
        private readonly IClientGroupRepository _clientGroupRepository;
        private readonly IComplianceCaseRepository _complianceCaseRepository;
        private readonly ITenderRepository _tenderRepository;
        private readonly ICaseLetterRepository _caseLetterRepository;
        private readonly IDocumentService _documentService;
        private readonly IEmailService _emailService;
        private readonly ISystemSettingService _systemSettingService;
        private readonly IWorkingDayCalculator _workingDayCalculator;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ClientGroupService> _logger;

        public ClientGroupService(
            IClientGroupRepository clientGroupRepository,
            IComplianceCaseRepository complianceCaseRepository,
            ITenderRepository tenderRepository,
            ICaseLetterRepository caseLetterRepository,
            IDocumentService documentService,
            IEmailService emailService,
            ISystemSettingService systemSettingService,
            IWorkingDayCalculator workingDayCalculator,
            IAuditService auditService,
            IUnitOfWork unitOfWork,
            ILogger<ClientGroupService> logger)
        {
            _clientGroupRepository = clientGroupRepository;
            _complianceCaseRepository = complianceCaseRepository;
            _tenderRepository = tenderRepository;
            _caseLetterRepository = caseLetterRepository;
            _documentService = documentService;
            _emailService = emailService;
            _systemSettingService = systemSettingService;
            _workingDayCalculator = workingDayCalculator;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // -----------------------------------------------------------------
        // Get or create
        // -----------------------------------------------------------------

        public async Task<ClientGroup> GetOrCreateDraftGroupAsync(
            string employerName,
            string? contactName,
            string? contactEmail,
            string? ccEmails,
            CancellationToken cancellationToken = default)
        {
            var normalized = EmployerNameNormalizer.Normalize(employerName);

            var existing = await _clientGroupRepository
                .GetDraftByEmployerAsync(normalized, cancellationToken);

            if (existing != null)
                return existing;

            var group = new ClientGroup
            {
                Id = Guid.NewGuid(),
                NormalizedEmployerName = normalized,
                EmployerName = employerName,
                ContactName = contactName,
                ContactEmail = contactEmail,
                CcEmails = ccEmails,
                Status = ClientGroupStatus.Draft,
                CreatedOn = DateTime.UtcNow
            };

            await _clientGroupRepository.AddAsync(group, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return group;
        }

        // -----------------------------------------------------------------
        // Add / remove cases
        // -----------------------------------------------------------------

        public async Task AddCaseAsync(
            Guid groupId,
            Guid complianceCaseId,
            CancellationToken cancellationToken = default)
        {
            var group = await _clientGroupRepository
                .GetByIdAsync(groupId, cancellationToken)
                ?? throw new InvalidOperationException("Client group not found.");

            if (group.Status != ClientGroupStatus.Draft)
                throw new InvalidOperationException(
                    "Cases can only be added to a draft client group.");

            // Idempotent - if the case is already in the group, do nothing.
            if (group.Cases.Any(c => c.ComplianceCaseId == complianceCaseId))
                return;

            group.Cases.Add(new ClientGroupCase
            {
                Id = Guid.NewGuid(),
                ClientGroupId = groupId,
                ComplianceCaseId = complianceCaseId,
                AddedOn = DateTime.UtcNow
            });

            await _clientGroupRepository.UpdateAsync(group, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveCaseAsync(
            Guid groupId,
            Guid complianceCaseId,
            CancellationToken cancellationToken = default)
        {
            var group = await _clientGroupRepository
                .GetByIdAsync(groupId, cancellationToken)
                ?? throw new InvalidOperationException("Client group not found.");

            if (group.Status != ClientGroupStatus.Draft)
                throw new InvalidOperationException(
                    "Cases can only be removed from a draft client group.");

            var link = group.Cases
                .FirstOrDefault(c => c.ComplianceCaseId == complianceCaseId);

            if (link == null)
                return;

            group.Cases.Remove(link);

            await _clientGroupRepository.UpdateAsync(group, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // -----------------------------------------------------------------
        // Issue
        // -----------------------------------------------------------------

        public async Task<Guid> IssueGroupAsync(
    Guid groupId,
    LetterType letterType,
    Guid issuedByUserId,
    CancellationToken cancellationToken = default)
        {
            var group = await _clientGroupRepository
                .GetByIdAsync(groupId, cancellationToken)
                ?? throw new InvalidOperationException("Client group not found.");

            if (group.Status != ClientGroupStatus.Draft)
                throw new InvalidOperationException(
                    "Only draft groups can be issued.");

            if (group.Cases.Count == 0)
                throw new InvalidOperationException(
                    "A client group must contain at least one case before it can be issued.");

            if (string.IsNullOrWhiteSpace(group.ContactEmail))
                throw new InvalidOperationException(
                    "The client group has no recipient email address.");

            var settings = await _systemSettingService.GetAsync();

            // Load the tenders for all cases in the group.
            var caseIds = group.Cases.Select(c => c.ComplianceCaseId).ToList();
            var cases = await _complianceCaseRepository
                .GetByIdsAsync(caseIds, cancellationToken);

            var tenderIds = cases.Select(c => c.TenderId).Distinct().ToList();
            var tenders = await _tenderRepository
                .GetByIdsAsync(tenderIds, cancellationToken);

            var tendersByCase = cases.ToDictionary(
                c => c.Id,
                c => tenders.FirstOrDefault(t => t.Id == c.TenderId));

            // ---------------------------------------------------------
            // Determine type and due date
            // ---------------------------------------------------------

            DateTime dueOn;

            if (letterType == LetterType.Instruction)
            {
                dueOn = await _workingDayCalculator.AddWorkingDaysAsync(
                    DateTime.UtcNow,
                    settings.InstructionLetterResponseWorkingDays,
                    cancellationToken);
            }
            else
            {
                dueOn = DateTime.UtcNow.AddDays(settings.ContraventionNoticeResponseDays);
            }

            // ---------------------------------------------------------
            // Build the Annexure A rows
            // ---------------------------------------------------------
            var annexureRows = new List<AnnexureRow>();
            var rowNumber = 1;

            foreach (var groupCase in group.Cases.OrderBy(c => c.AddedOn))
            {
                var tender = tendersByCase[groupCase.ComplianceCaseId];

                if (tender is null)
                    continue;

                annexureRows.Add(new AnnexureRow
                {
                    Number = rowNumber++,
                    ClientName = tender.Title,
                    TenderNumber = tender.TenderNumber,
                    IsAdvertised = tender.IsAdvertised,
                    IsRegistered = tender.IsRegisteredOnRoP,
                    IsCompleted = false // CompletionStatus is deferred
                });
            }

            // ---------------------------------------------------------
            // Generate the consolidated letter and the annexure
            // ---------------------------------------------------------
            var letterModel = new ConsolidatedLetterModel
            {
                Type = letterType,
                EmployerName = group.EmployerName,
                RecipientName = group.ContactName ?? group.EmployerName,
                RecipientEmail = group.ContactEmail!,
                CcEmails = group.CcEmails,
                HodName = group.ContactName ?? group.EmployerName,
                DepartmentAddress = string.Empty, // TODO: add an address field to ClientGroup
                DueOn = dueOn,
                TenderCount = annexureRows.Count
            };

            var annexureModel = new AnnexureModel
            {
                EmployerName = group.EmployerName,
                Rows = annexureRows
            };

            var letterDoc = await _documentService
                .GenerateConsolidatedLetterAsync(letterModel, cancellationToken);

            var annexureDoc = await _documentService
                .GenerateAnnexureAsync(annexureModel, cancellationToken);

            group.AnnexureFileName = annexureDoc.FileName;
            group.AnnexureFilePath = annexureDoc.FilePath;

            // ---------------------------------------------------------
            // Create the CaseLetter that represents the consolidated letter
            // ---------------------------------------------------------
            var letter = new CaseLetter
            {
                Id = Guid.NewGuid(),
                ComplianceCaseId = group.Cases.First().ComplianceCaseId, // "anchor" case
                ClientGroupId = group.Id,
                Type = letterType,
                LetterNumber = 1, // TODO: use ILetterNumberGenerator.GetNextNumberAsync(letterType)
                RecipientName = group.ContactName ?? group.EmployerName,
                RecipientEmail = group.ContactEmail!,
                FileName = letterDoc.FileName,
                FilePath = letterDoc.FilePath,
                SentOn = DateTime.UtcNow,
                ResponseDueOn = dueOn,
                EmailSent = false,
                CreatedOn = DateTime.UtcNow
            };

            await _caseLetterRepository.AddAsync(letter, cancellationToken);

            group.ConsolidatedLetterId = letter.Id;
            group.Status = ClientGroupStatus.Issued;
            group.IssuedOn = DateTime.UtcNow;
            group.ModifiedBy = issuedByUserId;
            group.ModifiedOn = DateTime.UtcNow;

            await _clientGroupRepository.UpdateAsync(group, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ---------------------------------------------------------
            // Email the letter + annexure
            // ---------------------------------------------------------
            try
            {
                await _emailService.SendAsync(new EmailMessageModel
                {
                    ToAddress = group.ContactEmail!,
                    ToName = group.ContactName ?? group.EmployerName,
                    CcAddress = group.CcEmails,
                    Subject = letterType == LetterType.Instruction
                        ? $"Instructional Letter: Compliance Assessment - {group.EmployerName}"
                        : $"Contravention Notice - {group.EmployerName}",
                    Body =
                        $"<p>Dear {group.ContactName ?? "Sir/Madam"},</p>" +
                        "<p>Please find attached the compliance communication and Annexure A " +
                        "listing the identified tenders.</p>" +
                        "<p>Regards,<br/>iTender Compliance</p>",
                    AttachmentPaths = new List<string>
                    {
                        letterDoc.FilePath,
                        annexureDoc.FilePath
                    }
                }, cancellationToken);

                letter.EmailSent = true;
                await _caseLetterRepository.UpdateAsync(letter, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to email consolidated letter for client group {GroupId}.",
                    group.Id);

                await _auditService.LogAsync(
                    AuditAction.Error,
                    AuditEntity.CaseLetter,
                    letter.Id,
                    $"Failed to email consolidated letter to {group.ContactEmail}: {ex.Message}",
                    issuedByUserId,
                    cancellationToken);
            }

            // ---------------------------------------------------------
            // Audit
            // ---------------------------------------------------------
            await _auditService.LogAsync(
                letterType == LetterType.Instruction
                    ? AuditAction.InstructionLetterSent
                    : AuditAction.ContraventionNoticeSent,
                AuditEntity.CaseLetter,
                letter.Id,
                $"{letterType} #{letter.LetterNumber} issued for {group.EmployerName} " +
                $"covering {annexureRows.Count} tender(s). Response due {dueOn:dd MMM yyyy}.",
                issuedByUserId,
                cancellationToken);

            _logger.LogInformation(
                "Client group {GroupId} issued as {LetterType}. {TenderCount} tender(s) covered.",
                group.Id, letterType, annexureRows.Count);

            return letter.Id;
        }

        public async Task UpdateRecipientAsync(
    Guid groupId,
    string? contactName,
    string? contactEmail,
    string? ccEmails,
    Guid modifiedByUserId,
    CancellationToken cancellationToken = default)
        {
            var group = await _clientGroupRepository
                .GetByIdAsync(groupId, cancellationToken)
                ?? throw new InvalidOperationException("Client group not found.");

            if (group.Status != ClientGroupStatus.Draft)
                throw new InvalidOperationException(
                    "Recipient details can only be changed on a draft group.");

            await _clientGroupRepository.UpdateGroupMetaAsync(
                groupId,
                contactName,
                contactEmail,
                ccEmails,
                modifiedByUserId,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}