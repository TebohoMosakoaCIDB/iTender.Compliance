namespace iTender.Compliance.Domain.Utilities
{
    public static class EmployerNameNormalizer
    {
        public static string Normalize(string? value)
            => string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : new string(
                    value.ToLowerInvariant()
                         .Where(char.IsLetterOrDigit)
                         .ToArray());
    }
}