using iTender.Compliance.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace iTender.Compliance.Application.DTOs
{
    public class ClientGroupDetailModel
    {
        public Guid Id { get; set; }
        public string EmployerName { get; set; } = string.Empty;
        public ClientGroupStatus Status { get; set; }
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? CcEmails { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? IssuedOn { get; set; }

        public List<ClientGroupTenderModel> Tenders { get; set; } = new();

        public Guid? ConsolidatedLetterId { get; set; }
        public string? ConsolidatedLetterFileName { get; set; }
        public string? ConsolidatedLetterFilePath { get; set; }
        public LetterType? LetterType { get; set; }
        public string? AnnexureFileName { get; set; }
        public string? AnnexureFilePath { get; set; }
    }

    public class ClientGroupTenderModel
    {
        public Guid ComplianceCaseId { get; set; }
        public Guid TenderId { get; set; }
        public string TenderNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool IsAdvertised { get; set; }
        public bool IsRegistered { get; set; }
        public DateTime ClosingDate { get; set; }
    }
}
