using iTender.Compliance.Application.Interfaces.Services;

namespace iTender.Compliance.Infrastructure.Services
{
    public class WorkClassificationValidator : IWorkClassificationValidator
    {
        // Known valid class codes (from your provided data)
        private static readonly HashSet<string> ValidClasses = new(StringComparer.OrdinalIgnoreCase)
        {
            "CE", "EB", "EP", "GB", "ME", "SB", "SC", "SD", "SE", "SF"
            // Add any other classes you have – you can fetch from a database or expand this list.
        };

        public Task<bool> ValidateAsync(string classOfWorks, string projectDescription, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(classOfWorks))
                return Task.FromResult(false); // missing class = invalid

            // Split by commas (may contain multiple classes)
            var codes = classOfWorks
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // Check each code against the valid list
            bool allValid = codes.All(c => ValidClasses.Contains(c));

            return Task.FromResult(allValid);
        }
    }
}
