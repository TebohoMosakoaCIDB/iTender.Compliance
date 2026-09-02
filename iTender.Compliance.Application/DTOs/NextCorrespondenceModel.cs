using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class NextCorrespondenceModel
    {
        public bool CanGenerate { get; set; }

        public CorrespondenceTemplateType? Type { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int ResponseHours { get; set; }

        public DateTime ResponseDueOn { get; set; }

        public string ResponsePeriodText { get; set; } = string.Empty;
    }
}