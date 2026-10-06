using iTender.Compliance.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace iTender.Compliance.Application.DTOs
{
    public class ConsolidatedLetterModel
    {
        public LetterType Type { get; set; }  // Instruction or ContraventionNotice
        public string EmployerName { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public string? CcEmails { get; set; }
        public string HodName { get; set; } = string.Empty;
        public string DepartmentAddress { get; set; } = string.Empty;
        public DateTime DueOn { get; set; }
        public int TenderCount { get; set; }
    }

    public class AnnexureModel
    {
        public string EmployerName { get; set; } = string.Empty;
        public List<AnnexureRow> Rows { get; set; } = new();
    }

    public class AnnexureRow
    {
        public int Number { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string TenderNumber { get; set; } = string.Empty;
        public bool IsAdvertised { get; set; }
        public bool IsRegistered { get; set; }
        public bool IsCompleted { get; set; }
    }
}
