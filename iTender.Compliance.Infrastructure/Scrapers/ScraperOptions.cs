namespace iTender.Compliance.Infrastructure.Scrapers
{
    public class ScraperOptions
    {
        public const string SectionName = "Scraper";

        public string BaseUrl { get; set; } = "http://localhost:8000";
        public int TimeoutSeconds { get; set; } = 180;
        public bool Enabled { get; set; } = true;
    }
}
