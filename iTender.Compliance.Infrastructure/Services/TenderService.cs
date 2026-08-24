using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Repositories;

namespace iTender.Compliance.Infrastructure.Services
{
    public class TenderService : ITenderService
    {
        private readonly ITenderRepository _tenderRepository;

        public TenderService(
            ITenderRepository tenderRepository)
        {
            _tenderRepository = tenderRepository;
        }

        public async Task<PagedResult<TenderDto>> SearchAsync(
    TenderSearchModel search,
    CancellationToken cancellationToken = default)
        {
            var result = await _tenderRepository.SearchAsync(
                search,
                cancellationToken);

            return new PagedResult<TenderDto>
            {
                Items = result.Items
                    .Select(MapTender)
                    .ToList(),

                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }

        private static TenderDto MapTender(Tender tender)
        {
            return new TenderDto
            {
                Id = tender.Id,

                EmployerTenderNumber = tender.TenderNumber,

                Title = tender.Title,

                EmployerName = tender.EmployerName,

                DateAdvertised = tender.AdvertisedDate,

                ClosingDateTime = tender.ClosingDate,

                AwardedDate = tender.AwardedDate,

                IsRegisteredOnRoP = tender.IsRegisteredOnRoP,

                HasComplianceCase = tender.ComplianceCase != null,

                ComplianceCaseStatus =
                    tender.ComplianceCase?.Status.ToString()
            };
        }

        public async Task<List<TenderDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var tenders =
                await _tenderRepository.GetAllAsync(cancellationToken);

            return tenders
                .Select(t => new TenderDto
                {
                    Id = t.Id,
                    EmployerTenderNumber = t.TenderNumber,
                    Title = t.Title,
                    EmployerName = t.EmployerName,
                    DateAdvertised = t.AdvertisedDate,
                    ClosingDateTime = t.ClosingDate,
                    AwardedDate = t.AwardedDate,
                    IsRegisteredOnRoP = t.IsRegisteredOnRoP,

                    HasComplianceCase = t.ComplianceCase != null,

                    ComplianceCaseStatus =
                        t.ComplianceCase?.Status.ToString()
                })
                .ToList();
        }

        public async Task<TenderDetailModelDto?> GetDetailAsync(
    Guid id,
    CancellationToken cancellationToken = default)
        {
            var tender =
                await _tenderRepository.GetDetailAsync(
                    id,
                    cancellationToken);

            if (tender == null)
                return null;

            return new TenderDetailModelDto
            {
                Id = tender.Id,

                TenderNumber = tender.TenderNumber,

                Title = tender.Title,

                Description = tender.Description,

                EmployerName = tender.EmployerName,

                ContactName = tender.ContactName,

                ContactEmail = tender.ContactEmail,

                ContactNumber = tender.ContactNumber,

                AdvertisedDate = tender.AdvertisedDate,

                ClosingDate = tender.ClosingDate,

                Source = tender.Source,

                TenderUrl = tender.TenderUrl,

                TenderSyncId = tender.TenderSyncId,

                IsConstruction = tender.IsConstruction,

                ClassOfWorks = tender.ClassOfWorks,

                AwardedDate = tender.AwardedDate,

                AwardValue = tender.AwardValue,

                WinningContractor = tender.WinningContractor,

                IsRegisteredOnRoP = tender.IsRegisteredOnRoP,

                RoPRegistrationDate = tender.RoPRegistrationDate,

                ComplianceCaseId =
                    tender.ComplianceCase?.Id,

                TenderSync = tender.TenderSync == null
                    ? null
                    : new TenderSyncDetailModel
                    {
                        Id = tender.TenderSync.Id,

                        StartedOn =
                            tender.TenderSync.StartedOn,

                        CompletedOn =
                            tender.TenderSync.CompletedOn,

                        Status =
                            tender.TenderSync.Status,

                        IsManual =
                            tender.TenderSync.IsManual,

                        StartedByUserId =
                            tender.TenderSync.StartedByUserId,

                        TotalRetrieved =
                            tender.TenderSync.TotalRetrieved,

                        TotalCompliant =
                            tender.TenderSync.TotalCompliant,

                        TotalNonCompliant =
                            tender.TenderSync.TotalNonCompliant,

                        CasesCreated =
                            tender.TenderSync.CasesCreated,

                        ErrorCount =
                            tender.TenderSync.ErrorCount,

                        Notes =
                            tender.TenderSync.Notes,

                        Logs = tender.TenderSync.Logs
                            .OrderByDescending(x => x.CreatedOn)
                            .Select(x => new TenderSyncLogModel
                            {
                                Type = x.Type,

                                Level = x.Level,

                                Title = x.Title,

                                Message = x.Message,

                                TenderNumber =
                                    x.TenderNumber

                            })
                            .ToList()
                    }
            };
        }

        public async Task<TenderDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var tender =
                await _tenderRepository.GetByIdAsync(
                    id,
                    cancellationToken);

            if (tender == null)
                return null;

            return new TenderDto
            {
                Id = tender.Id,
                EmployerTenderNumber = tender.TenderNumber,
                Title = tender.Title,
                EmployerName = tender.EmployerName,
                DateAdvertised = tender.AdvertisedDate,
                ClosingDateTime = tender.ClosingDate,
                AwardedDate = tender.AwardedDate,
                IsRegisteredOnRoP = tender.IsRegisteredOnRoP,

                HasComplianceCase = tender.ComplianceCase != null,

                ComplianceCaseStatus =
                    tender.ComplianceCase?.Status.ToString()
            };
        }
    }
}
