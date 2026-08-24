namespace iTender.Compliance.Client.Helpers
{
    public static class EnumExtensions
    {
        public static string ToDisplayName(this Enum value)
        {
            // Insert spaces before each capital letter, except the first
            return System.Text.RegularExpressions.Regex.Replace(
                value.ToString(),
                "(?<=[a-z])([A-Z])",
                " $1"
            );
        }
    }
}
