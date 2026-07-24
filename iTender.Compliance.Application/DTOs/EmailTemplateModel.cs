using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.DTOs
{
    public class EmailTemplateModel : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
