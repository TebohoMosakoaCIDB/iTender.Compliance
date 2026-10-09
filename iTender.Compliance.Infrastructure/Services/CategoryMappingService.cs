using iTender.Compliance.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

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

        // -------------------------------------------------------------
        // Per-class keywords used to check whether the advert text
        // supports a class claimed by the category mapping.
        // -------------------------------------------------------------
        public static readonly IReadOnlyDictionary<string, string[]> ClassKeywords =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["CE"] = new[]
            {
                "access road", "road", "street", "gravel road", "paved road",
                "asphalt", "reseal", "pothole", "culvert", "stormwater",
                "bridge", "river crossing", "washaway", "washaways",
                "pipeline", "pipelines", "pipe laying", "pipe network",
                "water reticulation", "water pipeline", "sewer", "sewerage",
                "pump station", "reservoir", "dam", "borehole",
                "water treatment", "wastewater", "drainage", "earthworks",
                "geotechnical", "civil", "civil engineering"
            },

                ["GB"] = new[]
            {
                "building", "school", "clinic", "hospital", "office block",
                "library", "hall", "toilet", "ablution", "classroom",
                "crèche", "creche", "housing", "warehouse"
            },

                ["EB"] = new[]
            {
                "substation", "overhead line", "ohtl", "electrification",
                "streetlight", "high mast", "transformer", "switchgear"
            },

                ["EP"] = new[]
            {
                "substation", "overhead line", "ohtl", "transmission",
                "distribution network", "feeder"
            },

                ["ME"] = new[]
            {
                "pump", "cooling tower", "boiler", "mechanical", "hvac",
                "chiller", "conveyor", "turbine"
            },

                ["SN"] = new[]
            {
                "roof waterproofing", "waterproofing", "damp proofing",
                "roof leak"
            },

                ["SJ"] = new[] { "piling", "pile foundation", "shaft sinking" },
                ["SE"] = new[] { "demolition", "blasting" },
                ["SF"] = new[] { "fire prevention", "fire protection",
                             "sprinkler system", "fire detection" },
                ["SH"] = new[] { "landscaping", "horticulture", "gardening" },
                ["SI"] = new[] { "lift", "elevator", "escalator" },
                ["SK"] = new[] { "road marking", "road sign", "signage" },
                ["SL"] = new[] { "structural steel", "steelwork", "steel structure" },
                ["SB"] = new[] { "asphalt overlay", "resurfacing", "supply and lay" },
                ["SC"] = new[] { "excavation", "shaft sinking", "lateral earth support" },
                ["SD"] = new[] { "corrosion protection", "cathodic", "anodic" },
                ["SG"] = new[] { "glazing", "curtain wall", "shop front" },
                ["SM"] = new[] { "timber building", "timber structure" },
                ["SO"] = new[] { "plumbing", "wet services", "water supply and drainage" }
                //["SQ"] = new[] { "development", "extension", "installation",
                //             "repair", "removal", "alteration", "dismantling" },
            };

        private Dictionary<string, (bool IsConstruction, string? ClassOfWork)> BuildMappings()
        {
            return new(StringComparer.OrdinalIgnoreCase)
    {
        // ---- Group A: clear construction mappings ----
        { "Civil engineering", (true, "CE") },
        { "Construction", (true, null) },
        { "Construction of buildings", (true, "GB") },
        { "Services: Civil", (true, "CE") },
        { "Specialised construction activities", (true, null) },
        { "Services: Building", (true, "GB") },
        { "Services: Electrical", (true, "EB") },
        { "Electricity, gas, steam and air conditioning", (true, "EP") },

        // ---- Group B: construction-adjacent, class depends on text ----
        { "Services to buildings and landscape activities", (true, null) },
        { "Water supply; sewerage, waste management and remediation activities", (true, null) },
        { "Water collection, treatment and supply", (true, null) },
        { "Mining and quarrying", (true, null) },
        { "Repair and installation of machinery and equipment", (true, null) },
        { "Remediation activities and other waste management services", (true, null) },
        { "Land transport and transport via pipelines", (true, null) },
        { "Sewerage", (true, null) },
        { "Waste collection, treatment and disposal activities; materials recovery", (true, null) },

        // ---- Group C: supplies (never construction) ----
        { "Supplies: Perishable Provisions", (false, null) },
        { "Disposals: General", (false, null) },
        { "Supplies: Computer Equipment", (false, null) },
        { "Supplies: Stationery/Printing", (false, null) },
        { "Supplies: Electrical Equipment", (false, null) },
        { "Supplies: General", (false, null) },
        { "Supplies: Clothing/Textiles/Footwear", (false, null) },
        { "Supplies: Medical", (false, null) },

        // ---- Group D: everything else (non-construction) ----
        { "Libraries, archives, museums and other cultural activities", (false, null) },
        { "Residential care activities", (false, null) },
        { "Manufacture of fabricated metal products, except machinery and equipment", (false, null) },
        { "Education", (false, null) },
        { "Financial and insurance activities", (false, null) },
        { "Scientific research and development", (false, null) },
        { "Activities of households as employers of domestic personnel", (false, null) },
        { "Mining support service activities", (false, null) },
        { "Manufacture of paper and paper products", (false, null) },
        { "Manufacture of furniture", (false, null) },
        { "Manufacture of chemicals and chemical products", (false, null) },
        { "Food and beverage service activities", (false, null) },
        { "Human health activities", (false, null) },
        { "Architectural and engineering activities; technical testing and analysis", (false, null) },
        { "Manufacture of basic metals", (false, null) },
        { "Air transport", (false, null) },
        { "Wholesale and retail trade and repair of motor vehicles and motorcycles", (false, null) },
        { "Mining of coal and lignite", (false, null) },
        { "Manufacture of machinery and equipment n.e.c.", (false, null) },
        { "Manufacture of coke and refined petroleum products", (false, null) },
        { "Accommodation", (false, null) },
        { "Other professional, scientific and technical activities", (false, null) },
        { "Manufacture of rubber and plastics products", (false, null) },
        { "Human health and social work activities", (false, null) },
        { "Programming and broadcasting activities", (false, null) },
        { "Manufacture of computer, electronic and optical products", (false, null) },
        { "Warehousing and support activities for transportation", (false, null) },
        { "Employment activities", (false, null) },
        { "Information and communication", (false, null) },
        { "Activities auxiliary to financial service and insurance activities.", (false, null) },
        { "Manufacture of electrical equipment", (false, null) },
        { "Administrative and support activities", (false, null) },
        { "Advertising and market research", (false, null) },
        { "Arts, entertainment and recreation", (false, null) },
        { "Other service activities", (false, null) },
        { "Activities of head offices; management consultancy activities", (false, null) },
        { "Water transport", (false, null) },
        { "Legal and accounting activities", (false, null) },
        { "Sports activities and amusement and recreation activities", (false, null) },
        { "Other personal service activities", (false, null) },
        { "Publishing activities", (false, null) },
        { "Real estate activities", (false, null) },
        { "Printing and reproduction of recorded media", (false, null) },
        { "Services: General", (false, null) },
        { "Security and investigation activities", (false, null) },
        { "Telecommunications", (false, null) },
        { "Motion picture, video and television programme production, sound recording and music publishing activities", (false, null) },
        { "Travel agency, tour operator, reservation service and related activities", (false, null) },
        { "Manufacturing", (false, null) },
        { "Other manufacturing", (false, null) },
        { "Office administrative, office support and other business support activities", (false, null) },
        { "Manufacture of other non-metallic mineral products", (false, null) },
        { "Rental and leasing activities", (false, null) },
        { "Transportation and storage", (false, null) },
        { "Computer programming, consultancy and related activities", (false, null) },
        { "Services: Functional (Including Cleaning and Security Services)", (false, null) },
        { "Information service activities", (false, null) },
        { "Creative, arts and entertainment activities", (false, null) },
        { "Manufacture of motor vehicles, trailers and semi-trailers", (false, null) },
        { "Financial service activities, except insurance and pension funding", (false, null) },
        { "Insurance, reinsurance and pension funding, except compulsory social security", (false, null) },
        { "Postal and courier activities", (false, null) },
        { "Services: Professional", (false, null) },
        { "Manufacture of textiles", (false, null) },
        { "Professional, scientific and technical activities", (false, null) },
        { "Agricultural Products and Services", (false, null) },
    };
        }

        public Task<(bool IsConstruction, string? ClassOfWork)> MapCategoryAsync(
            string? categoryName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return Task.FromResult((false, (string?)null));

            var trimmed = categoryName.Trim();

            if (_mappings.TryGetValue(trimmed, out var mapping))
                return Task.FromResult(mapping);

            // Fallback: keyword match on the category name itself.
            if (trimmed.Contains("construction", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("building", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult((true, (string?)null));

            if (trimmed.Contains("civil", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult((true, (string?)"CE"));

            if (trimmed.Contains("electrical", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult((true, (string?)"EB"));

            _logger.LogWarning(
                "Unknown category '{Category}' – treating as non-construction.",
                trimmed);

            return Task.FromResult((false, (string?)null));
        }
    }
}