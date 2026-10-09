// iTender.Compliance.Domain/Utilities/KeywordMatcher.cs
using System;
using System.Text.RegularExpressions;

namespace iTender.Compliance.Domain.Utilities
{
    public static class KeywordMatcher
    {
        /// <summary>
        /// Case-insensitive keyword match. Multi-word phrases use plain Contains;
        /// single words require word boundaries so "road" doesn't match "broad".
        /// </summary>
        public static bool ContainsKeyword(string haystack, string keyword)
        {
            if (string.IsNullOrWhiteSpace(haystack) || string.IsNullOrWhiteSpace(keyword))
                return false;

            if (keyword.Contains(' '))
                return haystack.Contains(keyword, StringComparison.OrdinalIgnoreCase);

            return Regex.IsMatch(
                haystack,
                $@"\b{Regex.Escape(keyword)}\b",
                RegexOptions.IgnoreCase);
        }

        /// <summary>True if any of the keywords match the haystack.</summary>
        public static bool ContainsAnyKeyword(string haystack, System.Collections.Generic.IEnumerable<string> keywords)
        {
            foreach (var k in keywords)
                if (ContainsKeyword(haystack, k))
                    return true;
            return false;
        }
    }
}