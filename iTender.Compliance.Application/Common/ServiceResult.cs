namespace iTender.Compliance.Application.Common
{
    public class ServiceResult
    {
        public bool Succeeded { get; set; }

        public List<string> Errors { get; set; } = [];

        public static ServiceResult Success() =>
            new()
            {
                Succeeded = true
            };

        public static ServiceResult Failed(params string[] errors) =>
            new()
            {
                Succeeded = false,
                Errors = errors.ToList()
            };

        public static ServiceResult Failed(IEnumerable<string> errors) =>
            new()
            {
                Succeeded = false,
                Errors = errors.ToList()
            };
    }
}
