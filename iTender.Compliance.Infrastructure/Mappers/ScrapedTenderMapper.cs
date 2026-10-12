using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Utilities;

namespace iTender.Compliance.Infrastructure.Mappers
{
    public class ScrapedTenderMapper
    {
        public Tender? Map(ScrapedTenderDto dto, Guid syncId)
        {
            if (string.IsNullOrWhiteSpace(dto.TenderNumber)
                || string.IsNullOrWhiteSpace(dto.Title))
            {
                return null;
            }

            return new Tender
            {
                Id = Guid.NewGuid(),
                TenderNumber = dto.TenderNumber.Trim(),
                Title = dto.Title.Trim(),
                Description = dto.Title.Trim(),
                EmployerName = dto.EmployerName ?? string.Empty,
                ContactName = dto.ContactPerson,
                ContactEmail = dto.ContactEmail,
                ContactNumber = dto.ContactTelephone,
                AdvertisedDate = ParseDate(dto.AdvertisedDate) ?? DateTime.UtcNow,
                ClosingDate = ParseDate(dto.ClosingDate) ?? DateTime.UtcNow,
                Source = "eTenders (Scraper)",
                TenderUrl = dto.TenderUrl ?? string.Empty,
                Province = dto.Province,
                TenderSyncId = syncId,
                ProcurementCategory = dto.Category,
                IsConstruction = true, // filtered later by the sync pipeline
                CreatedOn = DateTime.UtcNow,
                NormalizedTitle = TenderTitleNormalizer.Normalize(dto.Title),
                NormalizedEmployerName = EmployerNameNormalizer.Normalize(dto.EmployerName)
            };
        }

        private static DateTime? ParseDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return DateTime.TryParse(value, out var result)
                ? DateTime.SpecifyKind(result, DateTimeKind.Utc)
                : null;
        }
    }
}
