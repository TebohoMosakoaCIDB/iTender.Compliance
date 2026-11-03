using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class ClientGroupedReportRow
    {
        public Guid? GroupId { get; set; }
        public string EmployerName { get; set; } = string.Empty;

        public int TotalTenders { get; set; }
        public int CompliantTenders { get; set; }
        public int NonCompliantTenders { get; set; }
        public int OpenTenders { get; set; }

        public int CasesNewlyIdentified { get; set; }
        public int CasesVerificationRequired { get; set; }
        public int CasesNotAdvertised { get; set; }
        public int CasesDueForInstructionalLetter { get; set; }
        public int CasesDueForContraventionNotice { get; set; }
        public int CasesCorrectiveAction { get; set; }
        public int CasesNonCompliant { get; set; }
        public int CasesReferral { get; set; }
        public int CasesCleared { get; set; }
        public int CasesClosed { get; set; }

        public int CasesContraventionNoticeIssued { get; set; }
        public int CasesWaitingForResponse { get; set; }
        public int CasesReferredForEnforcement { get; set; }
        public int LettersIssued { get; set; }
        public DateTime? LastCorrespondenceDate { get; set; }
        public DateTime? LastCorrespondenceType { get; set; }

        public ClientGroupStatus? GroupStatus { get; set; }
        public Guid? AgentId { get; set; }
        public string? AgentName { get; set; }
    }
}
