using HtmlAgilityPack;
using iTender.Compliance.Application.Interfaces.Scrapers;
using iTender.Compliance.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace iTender.Compliance.Infrastructure.Scrapers
{
    public class TenderFlowScraper : IScraperService
    {
        public string Name => "TenderFlow";

        private readonly HttpClient _client;

        public TenderFlowScraper(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("TenderFlow");
        }

        public async Task<List<Tender>> ScrapeAsync(
            CancellationToken cancellationToken = default)
        {
            var tenders = new List<Tender>();

            for (int page = 1; page <= 50; page++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var url = page == 1
                    ? "https://www.tenderflow.co.za/categories/construction/"
                    : $"https://www.tenderflow.co.za/categories/construction/?page={page}";

                var html = await _client.GetStringAsync(url, cancellationToken);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var cards = doc.DocumentNode.SelectNodes("//div[contains(@class,'opportunity-card')]");

                if (cards == null || cards.Count == 0)
                    break;

                foreach (var card in cards)
                {
                    var anchor = card.SelectSingleNode(".//a[contains(@class,'text-dark')]");

                    var relativeLink = anchor?.GetAttributeValue("href", string.Empty);

                    var fullLink = string.IsNullOrWhiteSpace(relativeLink)
                        ? string.Empty
                        : new Uri(new Uri("https://www.tenderflow.co.za"), relativeLink).ToString();

                    DateTime.TryParse(
                        card.SelectSingleNode(".//i[contains(@class,'bi-calendar-plus')]/parent::span")
                            ?.InnerText?.Trim(),
                        out var advertisedDate);

                    DateTime.TryParse(
                        card.SelectSingleNode(".//i[contains(@class,'bi-calendar3')]/parent::span")
                            ?.InnerText?.Trim(),
                        out var closingDate);

                    var tender = new Tender
                    {
                        TenderNumber = card.SelectSingleNode(".//span[contains(@class,'fw-bold')]")
                            ?.InnerText?.Trim() ?? string.Empty,

                        Title = anchor?.InnerText?.Trim() ?? string.Empty,

                        EmployerName = "Unknown",

                        AdvertisedDate = advertisedDate,

                        ClosingDate = closingDate,

                        TenderUrl = fullLink
                    };

                    if (!string.IsNullOrWhiteSpace(tender.Title))
                    {
                        tenders.Add(tender);
                    }
                }
            }

            return tenders;
        }
    }
}
