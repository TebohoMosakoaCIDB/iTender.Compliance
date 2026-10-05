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
            CorrespondenceTemplateType.AGSAReferral,
            CorrespondenceTemplateType.InstructionLetter,
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
            CorrespondenceTemplateType.InstructionLetter =>
                "Instruction Letter",

            CorrespondenceTemplateType.ContraventionNotice =>
                "Contravention Notice",

            CorrespondenceTemplateType.AGSAReferral =>
                "AGSA Referral Instruction",

            _ => type.ToString()
        };
    }


    private static string GetDefaultSubject(
        CorrespondenceTemplateType type)
    {
        return type switch
        {
            CorrespondenceTemplateType.InstructionLetter =>
                "INSTRUCTIONAL LETTER: COMPLIANCE ASSESSMENT",

            CorrespondenceTemplateType.ContraventionNotice =>
                "CONTRAVENTION NOTICE IN TERMS OF THE CONSTRUCTION INDUSTRY DEVELOPMENT BOARD ACT AND CID REGULATIONS (NOTICE)",

            CorrespondenceTemplateType.AGSAReferral =>
                "AGSA Referral Instruction - Tender {TenderNumber}",

            _ => string.Empty
        };
    }


    private static string GetDefaultBody(CorrespondenceTemplateType type)
    {
        return type switch
        {
            CorrespondenceTemplateType.InstructionLetter => """
Tender Number: {TenderNumber}
Tender Title: {TenderTitle}
Employer: {EmployerName}
Basis: {Reason}

1. In terms of Construction Industry Development Board Act,38 of 2000 read with the Construction Industry Development Regulations, GG 26427 of 9 June 2004 (CID Regulations) as amended, collectively referred to herein as CIDB prescripts your attention is drawn to the fact that:  

The Construction Industry Development Board (“CIDB”) in terms of the Construction Industry Development Board Act,38 of 2000 (the Act) has conducted compliance monitoring on construction tenders issued under your department. 

1.1. Section 22 (3) read with Regulation 18 (1A) stipulates that Construction contracts/projects above the prescribed tender value of R200 000 (Two Hundred Thousand Rand) for public sector must be registered on the register of projects (ROP) within 21 working days from date of written acceptance of contractor’s offer to perform construction work, further, to report to the Board project completion, termination, renewal of construction contracts and the settlement of all amounts owing to the contractor within one calendar month in terms of Section. 21 
1.2. Regulation 24 provides that every client or employer inviting calls for expression of interest or soliciting competitive tenders in relation to construction works contracts which the value exceeds R30 0000.00 (Thirty Thousand Rand) must publish that invitation to tender on the Board’s website.  

2. CIDB through its compliance monitoring process has identified construction contracts/ tenders, quotations or expression of interests which were non-compliant with CIDB prescripts as stated above. (Refer to attached list “ANNEXURE ‘A’”). 

3. You are therefore instructed to comply with CIDB transcripts within 48 hours from the time this letter is transmitted electronically by email to your office, failing which, a contravention notice will be issued. 

Any queries herein may be addressed to Grace Rasivhaga (Compliance Officer) at 012 482 7388

Should you require assistance on Construction works online tender advertisement, the registration of awarded tenders and the reporting of completed or terminated construction works, please contact the  Register of Project Team (Ms Busang Phala, Mr. Karabo Mokone or Phindile Yengwa on 012 482 7325 / 012 482 7294 or at ROP queries ROPqueries@cidb.org.za / BusangT@cidb.org.za / KaraboM@cidb.org.za / PhindileS@cidb.org.za respectively. For non-compliance and CIDB prescripts interpretationqueries,Please Contact Adv. Marcia Mashele on 012 482 7308 or Marciam@cidb.org.za

We trust the above will receive your immediate attention 
Yours faithfully 

""",

            // NOTE: This is now only the SUBSTANTIVE, editable body of a Contravention Notice -
            // the letterhead (date, addressee block, Attention/email/Cc, "Dear ..." salutation),
            // the bold underlined notice title, and the closing signature block / footer are all
            // rendered by DocumentService.GenerateContraventionNoticeAsync from
            // SendContraventionNoticeModel and the relevant SystemSetting fields, matching the
            // structure of the real CIDB Contravention Notice this was modelled on. Edit the
            // numbered paragraphs below for wording only - placeholders available here are
            // {TenderNumber}, {TenderTitle}, {EmployerName}, {Reason}, {ResponseDueDate}, {CurrentDate}.
            CorrespondenceTemplateType.ContraventionNotice => """
Tender Number: {TenderNumber}
Tender Title: {TenderTitle}
Employer: {EmployerName}
Basis: {Reason}

1. The Construction Industry Development Board Act, 38 of 2000 (Act) read with the Construction Industry Development Regulations, GG 26427 of 9 June 2004 (CID Regulations) as amended, collectively referred to herein as CIDB prescripts, impose an obligation on all public sector employers procuring construction works to comply with the CIDB prescripts.

2. In monitoring compliance obligations as prescribed by CIDB prescripts, the Construction Industry Development Board ("Board") has through its monitoring process identified a construction tender, quotation or expression of interest that was either not advertised or published on the Board website in accordance with CID Regulation 24 and/or not registered the award in terms of CID Regulation 18 nor reported the completed or terminated contract in relation to the awarded construction works as obliged in terms of CID Regulation 21, thus contravening the CIDB prescripts.

3. The following identified tender, quotation or expression of interest failed to comply with the CIDB prescripts:

4. As a result of the identified contravention of the CIDB Prescripts, you are therefore instructed to ensure registration of the identified tender upon receipt of this Notice, by no later than {ResponseDueDate}. This period commences on the day this letter is transmitted electronically by email to the recipient. The registration of this tender with the CIDB will not exempt the Department from the legal liability prescribed in paragraph 5 below.

5. Enforcement action including Preliminary investigation and the subsequent formal inquiry respectively, will be taken by the Board for failure to comply with the CIDB prescripts and will be reported to the Auditor General of South Africa (AGSA) in terms of CID Regulations 21(4) without further notice.

Should you require assistance on Construction works online tender advertisement, the registration of awarded tenders and the reporting of completed or terminated construction works, please contact the  Register of Project Team (Ms Busang Phala, Mr. Karabo Mokone or Phindile Yengwa on 012 482 7325 / 012 482 7294 or at ROP queries ROPqueries@cidb.org.za / BusangT@cidb.org.za / KaraboM@cidb.org.za / PhindileS@cidb.org.za respectively. For non-compliance and CIDB prescripts interpretationqueries,Please Contact Adv. Marcia Mashele on 012 482 7308 or Marciam@cidb.org.za

We trust the above will receive your immediate attention 
Yours faithfully

""",

            CorrespondenceTemplateType.AGSAReferral => """
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

Kindly provide confirmation of the corrective action taken and a copy or evidence of the issued AGSA Referral.

Failure to correct the tender within the prescribed period may result in the issuing of a Contravention Notice and referral for further enforcement action.

{FooterText}

Prepared by:

{AgentName}

Compliance Officer

Approved by:

{Manager_Signature}

Regulatory Compliance Manager
""",

            _ => string.Empty
        };
    }
}