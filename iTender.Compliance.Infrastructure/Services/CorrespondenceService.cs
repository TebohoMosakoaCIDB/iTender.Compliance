using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace iTender.Compliance.Infrastructure.Services
{
    public class CorrespondenceService : ICorrespondenceService
    {
        private readonly IComplianceCaseRepository _caseRepository;
        private readonly ICorrespondenceTemplateRepository _templateRepository;
        private readonly ICaseLetterRepository _caseLetterRepository;
        private readonly IComplianceActionRepository _complianceActionRepository;

        public CorrespondenceService(
            IComplianceCaseRepository caseRepository,
            ICorrespondenceTemplateRepository templateRepository,
            ICaseLetterRepository caseLetterRepository,
            IComplianceActionRepository complianceActionRepository)
        {
            _caseRepository = caseRepository;
            _templateRepository = templateRepository;
            _caseLetterRepository = caseLetterRepository;
            _complianceActionRepository = complianceActionRepository;
        }

        public async Task<byte[]> GenerateErratumAsync(Guid complianceCaseId)
        {
            return await GenerateAsync(
                complianceCaseId,
                CorrespondenceTemplateType.Erratum);
        }

        public async Task<byte[]> GenerateInstructionalLetterAsync(
            Guid complianceCaseId)
        {
            return await GenerateAsync(
                complianceCaseId,
                CorrespondenceTemplateType.InstructionalLetter);
        }

        public async Task<byte[]> GenerateContraventionNoticeAsync(
            Guid complianceCaseId)
        {
            return await GenerateAsync(
                complianceCaseId,
                CorrespondenceTemplateType.ContraventionNotice);
        }
        private async Task<byte[]> GenerateAsync(
            Guid complianceCaseId,
            CorrespondenceTemplateType templateType)
        {
            var caseModel =
                await _caseRepository.GetDetailAsync(complianceCaseId);

            if (caseModel == null)
                throw new InvalidOperationException(
                    "Compliance case could not be found.");

            var template =
                await _templateRepository.GetActiveAsync(templateType);

            if (template == null)
                throw new InvalidOperationException(
                    $"No active template exists for {templateType}.");

            var responseDueDate = GetResponseDueDate(templateType);

            var subject = ReplacePlaceholders(
                template.Subject,
                caseModel,
                responseDueDate);

            var body = ReplacePlaceholders(
                template.Body,
                caseModel,
                responseDueDate);

            // Create the correspondence records BEFORE returning the PDF
            await CreateCorrespondenceRecordsAsync(
                caseModel,
                templateType,
                subject,
                responseDueDate);

            // Create compliance action
            await CreateComplianceActionAsync(
                complianceCaseId,
                templateType,
                responseDueDate,
                $"Correspondence generated using the {templateType} template.");

            return GeneratePdf(
                subject,
                body);
        }

        private static DateTime GetResponseDueDate(
            CorrespondenceTemplateType templateType)
        {
            return templateType switch
            {
                CorrespondenceTemplateType.Erratum =>
                    DateTime.UtcNow.AddHours(48),

                CorrespondenceTemplateType.InstructionalLetter =>
                    DateTime.UtcNow.AddHours(48),

                CorrespondenceTemplateType.ContraventionNotice =>
                    DateTime.UtcNow.AddDays(14),

                _ => throw new InvalidOperationException(
                    $"No response period configured for {templateType}.")
            };
        }

        private async Task CreateComplianceActionAsync(
    Guid complianceCaseId,
    CorrespondenceTemplateType templateType,
    DateTime? responseDueDate,
    string? comments = null)
        {
            var actionType = templateType switch
            {
                CorrespondenceTemplateType.InstructionalLetter
                    => ComplianceActionType.InstructionalLetterSent,

                CorrespondenceTemplateType.ContraventionNotice
                    => ComplianceActionType.ContraventionNoticeSent,

                CorrespondenceTemplateType.Erratum
                => ComplianceActionType.ErratumNoticeSent,

                _ => throw new InvalidOperationException(
                    $"No compliance action type is configured for template type '{templateType}'.")
            };

            var action = new ComplianceAction
            {
                Id = Guid.NewGuid(),

                ComplianceCaseId = complianceCaseId,

                ActionType = actionType,

                Status = ComplianceActionStatus.Completed,

                ActionDate = DateTime.UtcNow,

                ResponseDueDate = responseDueDate,

                CompletedDate = DateTime.UtcNow,

                Comments = comments
            };

            await _complianceActionRepository.AddAsync(action);
        }

        private async Task CreateCorrespondenceRecordsAsync(
    ComplianceCaseDetailModel model,
    CorrespondenceTemplateType templateType,
    string subject,
    DateTime responseDueDate)
        {
            var now = DateTime.UtcNow;

            var letterNumber =
                model.Letters.Any()
                    ? model.Letters.Max(x => x.LetterNumber) + 1
                    : 1;

            var letter = new CaseLetter
            {
                Id = Guid.NewGuid(),

                ComplianceCaseId = model.Id,

                LetterNumber = letterNumber,

                RecipientName = model.Tender.Employer,
                RecipientEmail = model.Tender.Employer,

                SentOn = now,

                ResponseDueOn = responseDueDate,

                RespondedOn = null
            };

            await _caseLetterRepository.AddAsync(letter);
        }

        private static byte[] GeneratePdf(
            string subject,
            string body)
        {
            using var stream = new MemoryStream();

            Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);

                    page.Margin(50);

                    page.DefaultTextStyle(
                        x => x.FontSize(10));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Height(45)
                                .Image("wwwroot/cidb-logo.png");

                            column.Item()
                                .Text("Compliance Monitoring")
                                .FontSize(10)
                                .FontColor("#666666");

                            column.Item()
                                .PaddingTop(8)
                                .LineHorizontal(1)
                                .LineColor("#B3202A");
                        });

                    page.Content()
                        .PaddingTop(30)
                        .Column(column =>
                        {
                            column.Item()
                                .Text(subject)
                                .Bold()
                                .FontSize(14);

                            column.Item()
                                .PaddingTop(25)
                                .Text(body);
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("iTender Compliance Monitoring");
                        });
                });
            })
            .GeneratePdf(stream);

            return stream.ToArray();
        }

        private static string ReplacePlaceholders(
            string content,
            ComplianceCaseDetailModel model,
            DateTime? responseDueDate)
        {
            return content
                .Replace(
                    "{TenderNumber}",
                    model.Tender.TenderNumber ?? string.Empty)
                .Replace(
                    "{TenderTitle}",
                    model.Tender.Title ?? string.Empty)
                .Replace(
                    "{EmployerName}",
                    model.Tender.Employer ?? string.Empty)
                .Replace(
                    "{CompanyName}",
                    model.Tender.Employer ?? string.Empty)
                .Replace(
                    "{ResponseDueDate}",
                    responseDueDate.HasValue
                        ? responseDueDate.Value.ToString("dd MMMM yyyy")
                        : string.Empty)
                .Replace(
                    "{ClosingDate}",
                    model.Tender.ClosingDate.ToString("dd MMMM yyyy"))
                .Replace(
                    "{AgentName}",
                    model.Case.Agent ?? string.Empty);
        }

    }
}
