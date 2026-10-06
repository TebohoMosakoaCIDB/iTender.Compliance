using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class ClientGroupListItemModel
    {
        public Guid Id { get; set; }
        public string EmployerName { get; set; } = string.Empty;
        public ClientGroupStatus Status { get; set; }
        public int CaseCount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? IssuedOn { get; set; }
        public LetterType? LetterType { get; set; }
    }
}
