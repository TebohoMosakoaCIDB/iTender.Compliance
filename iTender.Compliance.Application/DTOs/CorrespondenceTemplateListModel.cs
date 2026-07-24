using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class CorrespondenceTemplateListModel
    {
        public Guid Id { get; set; }

        public CorrespondenceTemplateType Type { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
