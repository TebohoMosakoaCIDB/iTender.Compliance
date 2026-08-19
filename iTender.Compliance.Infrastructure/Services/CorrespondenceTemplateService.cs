using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class CorrespondenceTemplateService : ICorrespondenceTemplateService
    {
        private readonly ICorrespondenceTemplateRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public CorrespondenceTemplateService(
            ICorrespondenceTemplateRepository repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CorrespondenceTemplateListModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var templates = await _repository.GetAllAsync(cancellationToken);

            return templates
                .Select(x => new CorrespondenceTemplateListModel
                {
                    Id = x.Id,
                    Type = x.TemplateType,
                    Name = x.Name,
                    Subject = x.Subject,
                    Body = x.Body,
                    IsActive = x.IsActive
                })
                .OrderBy(x => x.Type)
                .ToList();
        }

        public async Task<CorrespondenceTemplateModel?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var template = await _repository.GetByIdAsync(
                id,
                cancellationToken);

            if (template == null)
                return null;

            return new CorrespondenceTemplateModel
            {
                Id = template.Id,
                CreatedOn = template.CreatedOn,
                CreatedBy = template.CreatedBy,
                ModifiedOn = template.ModifiedOn,
                ModifiedBy = template.ModifiedBy,

                TemplateType = template.TemplateType,
                Name = template.Name,
                Subject = template.Subject,
                Body = template.Body,
                HeaderImagePath = template.HeaderImagePath,
                IsActive = template.IsActive
            };
        }

        public async Task UpdateAsync(
            SaveCorrespondenceTemplateModel model,
            CancellationToken cancellationToken = default)
        {
            var template = await _repository.GetByIdAsync(
                model.Id,
                cancellationToken);

            if (template == null)
                throw new InvalidOperationException("Correspondence template not found.");

            template.Name = model.Name;
            template.Subject = model.Subject;
            template.Body = model.Body;
            template.HeaderImagePath = model.HeaderImagePath;
            template.IsActive = model.IsActive;

            // Prevent changing the template type
            // template.TemplateType = model.TemplateType;

            await _repository.UpdateAsync(
                template,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<Guid> CreateAsync(
            CorrespondenceTemplateType type,
            CancellationToken cancellationToken = default)
        {
            // Prevent duplicates
            var existing = await _repository.GetByTypeAsync(
                type,
                cancellationToken);

            if (existing != null)
                return existing.Id;

            var template = new CorrespondenceTemplateModel
            {
                Name = GetDefaultName(type),
                TemplateType = type,
                Subject = GetDefaultSubject(type),
                Body = GetDefaultBody(type),
                IsActive = true,
                HeaderImagePath = "cidb-logo.png"
            };

            await _repository.AddAsync(
                template,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return template.Id;
        }

        #region Helpers
        private static string GetDefaultName(CorrespondenceTemplateType type)
        {
            return type switch
            {
                CorrespondenceTemplateType.InstructionLetter => "Instruction Letter",
                CorrespondenceTemplateType.ReminderLetter => "Reminder Letter",
                CorrespondenceTemplateType.CaseClosed => "Case Closed",
                _ => type.ToString()
            };
        }

        private static string GetDefaultSubject(CorrespondenceTemplateType type)
        {
            return type switch
            {
                CorrespondenceTemplateType.InstructionLetter =>
                    "Compliance Review Required - Tender {TenderNumber}",

                CorrespondenceTemplateType.ReminderLetter =>
                    "Reminder - Outstanding Compliance Response - Tender {TenderNumber}",

                CorrespondenceTemplateType.CaseClosed =>
                    "Compliance Case Closed - Tender {TenderNumber}",

                _ => string.Empty
            };
        }

        private static string GetDefaultBody(CorrespondenceTemplateType type)
        {
            return type switch
            {
                CorrespondenceTemplateType.InstructionLetter => """
Dear {CompanyName},

A compliance review has been initiated for the following tender:

Tender Number: {TenderNumber}
Tender Title: {TenderTitle}
Employer: {EmployerName}
Closing Date: {ClosingDate}

You are requested to submit the required compliance documentation on or before {ResponseDueDate}.

The information provided will be used to assess compliance with the applicable CIDB requirements. Failure to respond within the prescribed period may affect the outcome of the compliance review.

Should you require any clarification or assistance, please contact the assigned Compliance Agent.

Kind regards,

{FooterText}
""",

                CorrespondenceTemplateType.ReminderLetter => """
Dear {CompanyName},

This serves as a reminder that we have not yet received your response regarding the compliance review for the following tender.

Tender Number: {TenderNumber}
Tender Title: {TenderTitle}
Employer: {EmployerName}

Our records indicate that the requested compliance documentation remains outstanding.

Please submit the required information on or before {ResponseDueDate} to allow the compliance review to continue.

If you have already submitted the requested documentation, kindly disregard this reminder.

Should you require any clarification or assistance, please contact the assigned Compliance Agent.

Kind regards,

{FooterText}
""",

                CorrespondenceTemplateType.CaseClosed => """
Dear {CompanyName},

The compliance review for the following tender has been concluded.

Tender Number: {TenderNumber}
Tender Title: {TenderTitle}
Employer: {EmployerName}

The compliance case has now been closed.

Thank you for your cooperation throughout the compliance review process.

Should you require any further information regarding this matter, please contact the Compliance Department.

Kind regards,

{FooterText}
""",

                _ => string.Empty
            };
        }

        #endregion
    }
}