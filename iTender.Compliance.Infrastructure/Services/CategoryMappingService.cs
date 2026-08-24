using iTender.Compliance.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace iTender.Compliance.Infrastructure.Services
{
    public class CategoryMappingService : ICategoryMappingService
    {
        private readonly ILogger<CategoryMappingService> _logger;
        private readonly Dictionary<string, (bool IsConstruction, string? ClassOfWork)> _mappings;

        public CategoryMappingService(ILogger<CategoryMappingService> logger)
        {
            _logger = logger;
            _mappings = BuildMappings();
        }

        private Dictionary<string, (bool IsConstruction, string? ClassOfWork)> BuildMappings()
        {
            return new(StringComparer.OrdinalIgnoreCase)
            {
                // ---- Construction categories ----
                { "Construction of buildings", (true, "GB") },
                { "Civil engineering", (true, "CE") },
                { "Specialised construction activities", (true, null) }, // could be GB, CE, etc.
                { "Construction", (true, null) },
                { "Services: Building", (true, "GB") },
                { "Services: Civil", (true, "CE") },
                { "Services: Electrical", (true, "EB") },
                { "Services: Functional (Including Cleaning and Security Services)", (false, null) }, // not construction
                // Add more from the list if needed...

                // ---- Non-construction categories ----
                { "Education", (false, null) },
                { "Financial and insurance activities", (false, null) },
                { "Information service activities", (false, null) },
                { "Supplies: General", (false, null) },
                { "Supplies: Computer Equipment", (false, null) },
                { "Supplies: Stationery/Printing", (false, null) },
                { "Supplies: Clothing/Textiles/Footwear", (false, null) },
                { "Supplies: Medical", (false, null) },
                { "Services: General", (false, null) },
                { "Services: Professional", (false, null) },
                { "Travel agency, tour operator, reservation service and related activities", (false, null) },
                { "Architectural and engineering activities; technical testing and analysis", (false, null) },
                { "Legal and accounting activities", (false, null) },
                { "Security and investigation activities", (false, null) },
                { "Office administrative, office support and other business support activities", (false, null) },
                { "Information and communication", (false, null) },
                { "Telecommunications", (false, null) },
                { "Computer programming, consultancy and related activities", (false, null) },
                { "Advertising and market research", (false, null) },
                { "Professional, scientific and technical activities", (false, null) },
                { "Human health activities", (false, null) },
                { "Residential care activities", (false, null) },
                { "Accommodation", (false, null) },
                { "Food and beverage service activities", (false, null) },
                { "Arts, entertainment and recreation", (false, null) },
                { "Sports activities and amusement and recreation activities", (false, null) },
                { "Other service activities", (false, null) },
                { "Other personal service activities", (false, null) },
                { "Activities of households as employers of domestic personnel", (false, null) },
                { "Libraries, archives, museums and other cultural activities", (false, null) },
                { "Programming and broadcasting activities", (false, null) },
                { "Motion picture, video and television programme production", (false, null) },
                { "Creative, arts and entertainment activities", (false, null) },
                // ---- You can add the rest from the list, but default will treat unknown as non-construction ----
            };
        }

        public Task<(bool IsConstruction, string? ClassOfWork)> MapCategoryAsync(string? categoryName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return Task.FromResult((false, (string?)null));

            var trimmed = categoryName.Trim();

            if (_mappings.TryGetValue(trimmed, out var mapping))
                return Task.FromResult(mapping);

            // Fallback: search for keywords
            if (trimmed.Contains("construction", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("building", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult((true, "GB"));

            if (trimmed.Contains("civil", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult((true, "CE"));

            if (trimmed.Contains("electrical", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult((true, "EB"));

            // Unknown – treat as non-construction
            _logger.LogWarning("Unknown category '{Category}' – treating as non-construction.", trimmed);
            return Task.FromResult((false, (string?)null));
        }
    }
}
