using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class CorrespondenceTemplateModel : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string? HeaderImagePath { get; set; }
        public CorrespondenceTemplateType TemplateType { get; set; }
    }
}
