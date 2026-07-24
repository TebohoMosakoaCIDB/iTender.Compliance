namespace iTender.Compliance.Application.DTOs
{
    public class TenderDto
    {
        public Guid Id { get; set; }

        public string? EmployerTenderNumber { get; set; }

        public string? Title { get; set; }

        public string? EmployerName { get; set; }

        public DateTime? DateAdvertised { get; set; }

        public DateTime? ClosingDateTime { get; set; }

        public Guid? CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }

        public List<ContactForTenderDto> ContactPerson { get; set; } = [];
    }
}
