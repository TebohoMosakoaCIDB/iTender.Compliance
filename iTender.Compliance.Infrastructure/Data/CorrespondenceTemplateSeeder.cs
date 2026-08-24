using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Persistence.Seeders;

public static class CorrespondenceTemplateSeeder
{
    public static async Task SeedAsync(
        ComplianceDbContext context)
    {
        foreach (var type in new[]
        {
            CorrespondenceTemplateType.Erratum,
            CorrespondenceTemplateType.InstructionalLetter,
            CorrespondenceTemplateType.ContraventionNotice
        })
        {
            var exists = await context.CorrespondenceTemplates
                .AnyAsync(x => x.Type == type);

            if (exists)
                continue;

            context.CorrespondenceTemplates.Add(
                new CorrespondenceTemplateModel
                {
                    Id = Guid.NewGuid(),
                    Name = GetDefaultName(type),
                    Type = type,
                    Status = CorrespondenceTemplateStatus.Approved,
                    Version = 1,
                    Subject = GetDefaultSubject(type),
                    Body = GetDefaultBody(type),
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = null,
                    CreatedBy = "SYSTEM",
                    UpdatedBy = null,
                    ApprovedOn = DateTime.UtcNow,
                    ApprovedBy = "SYSTEM",
                    ApprovalComments = "Initial system template."
                });
        }

        await context.SaveChangesAsync();
    }


    private static string GetDefaultName(
        CorrespondenceTemplateType type)
    {
        return type switch
        {
            CorrespondenceTemplateType.InstructionalLetter =>
                "Instruction Letter",

            CorrespondenceTemplateType.ContraventionNotice =>
                "Contravention Notice",

            CorrespondenceTemplateType.Erratum =>
                "Erratum Instruction",

            _ => type.ToString()
        };
    }


    private static string GetDefaultSubject(
        CorrespondenceTemplateType type)
    {
        return type switch
        {
            CorrespondenceTemplateType.InstructionalLetter =>
                "Instruction Letter - Compliance Action Required - Tender {TenderNumber}",

            CorrespondenceTemplateType.ContraventionNotice =>
                "Contravention Notice - Tender {TenderNumber}",

            CorrespondenceTemplateType.Erratum =>
                "Erratum Instruction - Tender {TenderNumber}",

            _ => string.Empty
        };
    }


    private static string GetDefaultBody(CorrespondenceTemplateType type)
    {
        return type switch
        {
            CorrespondenceTemplateType.InstructionalLetter => """
Dear {CompanyName},

The CIDB has identified a compliance matter relating to the following project:

Tender Number: {TenderNumber}
Tender Title: {TenderTitle}
Employer: {EmployerName}

You are hereby instructed to address the identified compliance matter and provide the required response within the prescribed period.

Response Due Date: {ResponseDueDate}

Kind regards,

{FooterText}

{Agent_Signature}
""",

            CorrespondenceTemplateType.ContraventionNotice => """
Dear {CompanyName},

NOTICE OF CONTRAVENTION

The CIDB has identified a contravention relating to the following tender/project:

Tender Number: {TenderNumber}
Tender Title: {TenderTitle}
Employer: {EmployerName}

The identified non-compliance requires your attention and corrective action.

You are required to provide a written response within 14 days of receipt of this notice.

Response Due Date: {ResponseDueDate}

Failure to comply may result in the matter being referred for further enforcement action.

Kind regards,

{FooterText}

{Agent_Signature}
""",

            CorrespondenceTemplateType.Erratum => """
Dear {CompanyName},

INSTRUCTION TO CORRECT TENDER ADVERTISEMENT

The CIDB has identified a compliance matter relating to the following tender:

Tender Number: {TenderNumber}
Tender Title: {TenderTitle}
Employer: {EmployerName}

The tender advertisement has been identified as containing a requirement that does not comply with the applicable CIDB requirements.

As the tender is currently open, you are instructed to correct the tender advertisement by issuing an erratum.

The required correction must be made within 48 hours of receipt of this instruction.

Response Due Date: {ResponseDueDate}

Kindly provide confirmation of the corrective action taken and a copy or evidence of the issued erratum.

Failure to correct the tender within the prescribed period may result in the issuing of a Contravention Notice and referral for further enforcement action.

Kind regards,

{FooterText}

{Agent_Signature}
""",

            _ => string.Empty
        };
    }
}