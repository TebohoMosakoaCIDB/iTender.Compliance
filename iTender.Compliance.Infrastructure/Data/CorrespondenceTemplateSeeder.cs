using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Persistence.Seeders;

public static class CorrespondenceTemplateSeeder
{
    public static async Task SeedAsync(ComplianceDbContext context)
    {
        foreach (var type in Enum.GetValues<CorrespondenceTemplateType>())
        {
            var exists = await context.CorrespondenceTemplates
                .AnyAsync(x => x.TemplateType == type);

            if (exists)
                continue;

            context.CorrespondenceTemplates.Add(new CorrespondenceTemplateModel
            {
                Id = Guid.NewGuid(),
                Name = GetDefaultName(type),
                Subject = GetDefaultSubject(type),
                Body = GetDefaultBody(type),
                TemplateType = type,
                IsActive = true,
                HeaderImagePath = null
            });
        }

        await context.SaveChangesAsync();
    }

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
}