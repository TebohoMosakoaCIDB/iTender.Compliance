using iTender.Compliance.Application.DTOs.Etenders;
using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Infrastructure.Mappers
{
    public class EtendersTenderMapper
    {
        public Tender Map(
            EtendersRelease release,
            Guid tenderSyncId)
        {
            var sourceTender = release.Tender
                ?? throw new InvalidOperationException(
                    "eTenders release does not contain tender information.");

            return new Tender
            {
                TenderNumber = sourceTender.Title ?? sourceTender.Id ?? string.Empty,

                Title = sourceTender.Description
                        ?? sourceTender.Title
                        ?? string.Empty,

                Description = sourceTender.Description,

                EmployerName =
                    sourceTender.ProcuringEntity?.Name
                    ?? release.Buyer?.Name
                    ?? string.Empty,

                ContactName = sourceTender.ContactPerson?.Name,
                ContactEmail = sourceTender.ContactPerson?.Email,
                ContactNumber = sourceTender.ContactPerson?.TelephoneNumber,

                AdvertisedDate =
                    sourceTender.TenderPeriod?.StartDate
                    ?? release.Date
                    ?? DateTime.UtcNow,

                ClosingDate =
                    sourceTender.TenderPeriod?.EndDate
                    ?? DateTime.UtcNow,

                Source = "eTenders",

                TenderUrl = BuildTenderUrl(sourceTender),

                ExternalId = sourceTender.Id,
                Ocid = release.Ocid,

                ProcurementCategory = sourceTender.Category,
                Province = sourceTender.Province,
                DeliveryLocation = sourceTender.DeliveryLocation,

                ProcurementMethod = sourceTender.ProcurementMethod,
                ProcurementMethodDetails =
                    sourceTender.ProcurementMethodDetails,

                TenderStatus = sourceTender.Status,
                SpecialConditions = sourceTender.SpecialConditions,

                SourceDocumentUrl =
                    sourceTender.Documents
                        .FirstOrDefault(d =>
                            !string.IsNullOrWhiteSpace(d.Url))
                        ?.Url,

                TenderSyncId = tenderSyncId,

                IsConstruction = true
            };
        }

        private static string BuildTenderUrl(
            EtendersTender tender)
        {
            if (tender.Documents.Count > 0)
            {
                var document = tender.Documents
                    .FirstOrDefault(d =>
                        !string.IsNullOrWhiteSpace(d.Url));

                if (!string.IsNullOrWhiteSpace(document?.Url))
                    return document.Url;
            }

            return $"https://www.etenders.gov.za/";
        }
    }
}
