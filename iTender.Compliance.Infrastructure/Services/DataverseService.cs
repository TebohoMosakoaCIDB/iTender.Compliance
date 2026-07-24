using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using System.Net.Http.Json;

namespace iTender.Compliance.Infrastructure.Services
{
    public class DataverseService : IDataverseService
    {
        private readonly HttpClient _httpClient;
        private readonly ICurrentUserService _currentUser;

        public DataverseService(HttpClient httpClient, ICurrentUserService currentUser)
        {
            _httpClient = httpClient;
            _currentUser = currentUser;
        }

        public async Task<List<Tender>> GetAdvertisedTendersAsync(
            CancellationToken cancellationToken = default)
        {
            var tenders =
                await _httpClient.GetFromJsonAsync<List<TenderDto>>(
                    "Tenders",
                    cancellationToken);

            foreach(var tender in tenders)
            {
                tender.CreatedBy = _currentUser.UserId;
                tender.ModifiedBy = _currentUser.UserId;
            }

            return tenders?
                .Select(Map)
                .ToList()
                ?? [];
        }

        private static Tender Map(TenderDto dto)
        {
            var contact = dto.ContactPerson.FirstOrDefault();

            return new Tender
            {
                TenderNumber = dto.EmployerTenderNumber ?? "",
                Title = dto.Title ?? "",
                EmployerName = dto.EmployerName ?? "",
                AdvertisedDate = dto.DateAdvertised ?? DateTime.MinValue,
                ClosingDate = dto.ClosingDateTime ?? DateTime.MinValue,

                ContactName = contact?.PersonToQuery,
                ContactEmail = contact?.Email,
                ContactNumber = contact?.TelephoneNumber,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy,
                ModifiedOn = DateTime.UtcNow,
                ModifiedBy = dto.ModifiedBy,
            };
        }
    }
}
