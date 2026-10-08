namespace iTender.Compliance.Domain.Utilities
{
    public static class TenderTitleNormalizer
    {
        public static string? Normalize(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return null;

            var t = title.Trim().ToLowerInvariant();

            // collapse whitespace
            t = System.Text.RegularExpressions.Regex.Replace(t, @"\s+", " ");

            // strip leading articles so "The construction of X" == "construction of X"
            t = System.Text.RegularExpressions.Regex.Replace(t, @"^(the|a|an)\s+", "");

            return t;
        }
    }
}
