using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Repositories;

namespace iTender.Compliance.Infrastructure.Services
{
    public class ComplianceService : IComplianceService
    {
        private readonly IComplianceCaseRepository _complianceCaseRepository;
        private readonly ICaseLetterRepository _caseLetterRepository;
        private readonly INotificationService _notificationService;
        private readonly IAgentRepository _agentRepository;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public ComplianceService(
            IComplianceCaseRepository complianceCaseRepository,
            ICaseLetterRepository caseLetterRepository,
            IAgentRepository agentRepository,
            IAuditService auditService,
            INotificationService notificationService,
            ICurrentUserService currentUser,
            IUnitOfWork unitOfWork)
        {
            _complianceCaseRepository = complianceCaseRepository;
            _caseLetterRepository = caseLetterRepository;
            _notificationService = notificationService;
            _agentRepository = agentRepository;
            _auditService = auditService;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task AssignAgentAsync(
            Guid complianceCaseId,
            Guid agentId,
            CasePriority priority,
            string? comments,
            CancellationToken cancellationToken = default)
        {
            var complianceCase = await _complianceCaseRepository.GetByIdAsync(
                complianceCaseId,
                cancellationToken);

            if (complianceCase == null)
                throw new InvalidOperationException("Compliance case not found.");

            complianceCase.AgentId = agentId;
            complianceCase.Status = CaseStatus.Assigned;
            complianceCase.ModifiedOn = DateTime.UtcNow;
            complianceCase.Priority = priority;
            complianceCase.Comments = comments;
            complianceCase.AssignedOn = DateTime.UtcNow;

            await _complianceCaseRepository.UpdateAsync(complianceCase, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                 AuditAction.Assigned,
                 AuditEntity.ComplianceCase,
                 complianceCaseId,
                $"Case assigned. Priority set to {priority}.",
                _currentUser.UserId,
                cancellationToken);

            var Agent = _agentRepository.GetByIdAsync(agentId);

            await _notificationService.NotifyAsync(new CreateNotificationModel
            {
                UserId = Agent.Result.UserId,
                Title = "New Case Assigned",
                Message = $"Tender {complianceCase.Tender.TenderNumber} has been assigned to you.",
                Type = NotificationType.Information,
                Url = $"/cases/{complianceCase.Id}"
            });
        }

        public async Task MarkCompliantAsync(
            Guid complianceCaseId,
            string? comments,
            CancellationToken cancellationToken = default)
        {
            var complianceCase = await _complianceCaseRepository.GetByIdAsync(
                complianceCaseId,
                cancellationToken);

            if (complianceCase == null)
                throw new InvalidOperationException("Compliance case not found.");

            complianceCase.Outcome = ComplianceOutcome.Compliant;
            complianceCase.Comments = comments;
            complianceCase.ModifiedOn = DateTime.UtcNow;

            await _complianceCaseRepository.UpdateAsync(complianceCase, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.Updated,
                AuditEntity.ComplianceCase,
                complianceCase.Id,
                "Case marked as compliant.",
                cancellationToken: cancellationToken);
        }

        public async Task MarkNonCompliantAsync(
            Guid complianceCaseId,
            string? comments,
            CancellationToken cancellationToken = default)
        {
            var complianceCase = await _complianceCaseRepository.GetByIdAsync(
                complianceCaseId,
                cancellationToken);

            if (complianceCase == null)
                throw new InvalidOperationException("Compliance case not found.");

            complianceCase.Outcome = ComplianceOutcome.NonCompliant;
            complianceCase.Comments = comments;
            complianceCase.ModifiedOn = DateTime.UtcNow;

            await _complianceCaseRepository.UpdateAsync(complianceCase, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.Updated,
                AuditEntity.ComplianceCase,
                complianceCase.Id,
                "Case marked as non-compliant.",
                cancellationToken: cancellationToken);
        }

        public async Task CloseCaseAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default)
        {
            var complianceCase = await _complianceCaseRepository.GetByIdAsync(
                complianceCaseId,
                cancellationToken);

            if (complianceCase == null)
                throw new InvalidOperationException("Compliance case not found.");

            complianceCase.Status = CaseStatus.Closed;
            complianceCase.ClosedDate = DateTime.UtcNow;
            complianceCase.ModifiedOn = DateTime.UtcNow;

            await _complianceCaseRepository.UpdateAsync(complianceCase, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.CaseClosed,
                AuditEntity.ComplianceCase,
                complianceCase.Id,
                "Compliance case closed.",
                cancellationToken: cancellationToken);
        }

        public async Task<NextCorrespondenceModel?> GetNextCorrespondenceAsync(Guid complianceCaseId)
        {
            var model = await _complianceCaseRepository.GetDetailAsync(complianceCaseId);

            if (model == null)
                return null;

            if (model.Case.Status == "Closed")
                return null;

            if (!model.Findings.Any(f => !f.IsResolved))
                return null;

            var latestLetter = model.Letters
                .OrderByDescending(x => x.LetterNumber)
                .FirstOrDefault();

            var finding = model.Findings
                .Where(x => !x.IsResolved)
                .OrderBy(x => x.IdentifiedAt)
                .First();

            /*
             * STREAM 1 & 2
             */
            if (finding.Stream == ComplianceStream.ClassOfWorks || finding.Stream == ComplianceStream.Advertisement)
            {
                if (finding.TenderStatusAtCheck == TenderStatus.Open)
                {
                    if (latestLetter == null)
                    {
                        return new NextCorrespondenceModel
                        {
                            CanGenerate = true,

                            Type = CorrespondenceTemplateType.InstructionLetter,

                            Title = "Issue Erratum Instruction",

                            Description =
                                "The tender is still open. The client must correct the non-compliance by issuing an erratum within 48 hours.",

                            ResponseHours = 48,

                            ResponsePeriodText = "48 hours"
                        };
                    }
                }

                if (finding.TenderStatusAtCheck == TenderStatus.Closed)
                {
                    return new NextCorrespondenceModel
                    {
                        CanGenerate = true,

                        Type = CorrespondenceTemplateType.ContraventionNotice,

                        Title = "Issue Contravention Notice",

                        Description =
                            "The tender is closed and the identified non-compliance requires a Contravention Notice.",

                        ResponseHours = 14 * 24,

                        ResponsePeriodText = "14 days"
                    };
                }
            }

            /*
             * STREAM 3
             *
             * Award identified but project is not registered on RoP.
             */
            if (finding.Stream == ComplianceStream.RopRegistration)
            {
                if (latestLetter == null)
                {
                    return new NextCorrespondenceModel
                    {
                        CanGenerate = true,
                        Type = CorrespondenceTemplateType.InstructionLetter,
                        Title = "Issue Instructional Letter",
                        Description =
                            "The awarded project has not been identified on the Register of Projects. An Instructional Letter must be issued to the client.",
                        ResponseHours = 48,
                        ResponsePeriodText = "48 hours"
                    };
                }

                /*
                 * IL was already issued and no response/compliance.
                 * Next step = CN.
                 */
                if (latestLetter.LetterNumber == 1 &&
                    !latestLetter.RespondedOn.HasValue)
                {
                    return new NextCorrespondenceModel
                    {
                        CanGenerate = true,
                        Type = CorrespondenceTemplateType.ContraventionNotice,
                        Title = "Issue Contravention Notice",
                        Description =
                            "The client has not complied with the Instructional Letter. A Contravention Notice may now be issued.",
                        ResponseHours = 14 * 24,
                        ResponsePeriodText = "14 days"
                    };
                }
            }

            return null;
        }

        public async Task RequestExtensionAsync(
           RequestExtensionModel model,
           CancellationToken cancellationToken = default)
        {
            if (model.AdditionalDays <= 0)
                throw new InvalidOperationException("Extension days must be greater than zero.");

            var complianceCase = await _complianceCaseRepository.GetByIdAsync(
                model.ComplianceCaseId,
                cancellationToken);

            if (complianceCase == null)
                throw new InvalidOperationException("Compliance case not found.");

            if (complianceCase.Status == CaseStatus.Closed)
                throw new InvalidOperationException("Cannot extend a closed case.");

            // Must be requested in writing against the currently outstanding letter.
            var outstandingLetter = await _complianceCaseRepository.GetLatestOutstandingAsync(
                complianceCase.Id,
                cancellationToken);

            if (outstandingLetter == null)
                throw new InvalidOperationException(
                    "There is no outstanding letter on this case to extend.");

            var newDueOn = outstandingLetter.ResponseDueOn.AddDays(model.AdditionalDays);

            outstandingLetter.ResponseDueOn = newDueOn;
            outstandingLetter.ModifiedOn = DateTime.UtcNow;

            await _caseLetterRepository.UpdateAsync(outstandingLetter, cancellationToken);

            complianceCase.ExtensionDays = (complianceCase.ExtensionDays ?? 0) + model.AdditionalDays;
            complianceCase.ExtendedDueOn = newDueOn;
            complianceCase.ModifiedOn = DateTime.UtcNow;

            await _complianceCaseRepository.UpdateAsync(complianceCase, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.ExtensionApproved,
                AuditEntity.ComplianceCase,
                complianceCase.Id,
                $"Extension of {model.AdditionalDays} day(s) granted. New response due date: {newDueOn:dd MMM yyyy}. Reason: {model.Reason}",
                _currentUser.UserId,
                cancellationToken);

            if (complianceCase.AgentId.HasValue)
            {
                await _notificationService.NotifyAsync(new CreateNotificationModel
                {
                    UserId = complianceCase.AgentId,
                    Title = "Response Deadline Extended",
                    Message = $"The response deadline for this case has been extended by {model.AdditionalDays} day(s), now due {newDueOn:dd MMM yyyy}.",
                    Type = NotificationType.Information,
                    Url = $"/cases/{complianceCase.Id}"
                }, cancellationToken);
            }
        }
    }
}
