using iTender.Application.DTOs;
using iTender.Compliance.Application.Filters;
using iTender.Compliance.Application.Interfaces;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Scrapers;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Mappers;
using Microsoft.Extensions.Configuration;

namespace iTender.Compliance.Infrastructure.Services
{
    public class SynchronizationService : ISynchronizationService
    {
        private readonly ITenderRepository _tenderRepository;
        private readonly ITenderSyncRepository _tenderSyncRepository;
        private readonly IComplianceCaseRepository _complianceCaseRepository;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenderDiscoveryAgent _openAI;
        private readonly IEnumerable<IScraperService> _scrapers;
        private readonly IDataverseService _dataverseService;
        private readonly ICurrentUserService _currentUser;
        private readonly ITenderSyncLogRepository _syncLogRepository;
        private readonly IConfiguration _configuration;
        private readonly IAutoAssignmentService _autoAssignmentService;
        private readonly ISystemSettingRepository _systemSettingRepository;
        private readonly IComplianceProcessingService _complianceProcessingService;
        private readonly IEtendersClient _etendersClient;
        private readonly EtendersConstructionFilter _etendersConstructionFilter;
        private readonly EtendersTenderMapper _etendersTenderMapper;
        public SynchronizationService(
            IEnumerable<IScraperService> scrapers,
            IDataverseService dataverseService,
            ITenderRepository tenderRepository,
            ITenderSyncRepository tenderSyncRepository,
            IComplianceCaseRepository complianceCaseRepository,
            IAuditService auditService,
            ISystemSettingRepository systemSettingRepository,
        ICurrentUserService currentUser,
            ITenderSyncLogRepository syncLogRepository,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ITenderDiscoveryAgent openAI,
            IComplianceProcessingService complianceProcessingService,
            IAutoAssignmentService autoAssignmentService,
            IEtendersClient etendersClient,
            EtendersConstructionFilter etendersConstructionFilter,
            EtendersTenderMapper etendersTenderMapper)
        {
            _scrapers = scrapers;
            _currentUser = currentUser;
            _tenderRepository = tenderRepository;
            _tenderSyncRepository = tenderSyncRepository;
            _complianceCaseRepository = complianceCaseRepository;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
            _dataverseService = dataverseService;
            _syncLogRepository = syncLogRepository;
            _configuration = configuration;
            _autoAssignmentService = autoAssignmentService;
            _systemSettingRepository = systemSettingRepository;
            _complianceProcessingService = complianceProcessingService;
            _openAI = openAI;
            _etendersClient = etendersClient;
            _etendersConstructionFilter = etendersConstructionFilter;
            _etendersTenderMapper = etendersTenderMapper;
        }

        public async Task SynchronizeAsync(bool isManual, CancellationToken cancellationToken = default)
        {
            var executionUserId = GetExecutionUserId(isManual);

            var sync = new TenderSync
            {
                StartedOn = DateTime.UtcNow,
                IsManual = isManual,
                StartedByUserId = executionUserId,
                Status = SyncStatus.Running
            };

            await _tenderSyncRepository.AddAsync(sync, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await LogAsync(
                sync.Id,
                SyncLogType.Information,
                SyncLogLevel.Information,
                "Synchronization Started",
                isManual
                    ? "Manual synchronization started."
                    : "Scheduled synchronization started.",
                cancellationToken: cancellationToken);

            try
            {
                var scrapedTenders = new List<Tender>();

                foreach (var scraper in _scrapers)
                {
                    var tenders = await scraper.ScrapeAsync(cancellationToken);

                    scrapedTenders.AddRange(tenders);

                    //var clientTenders = await _openAI.FindTendersAsync(
                    //        new DateTime(2026, 8, 23),
                    //        DateTime.Now);

                    await LogAsync(
                        sync.Id,
                        SyncLogType.Information,
                        SyncLogLevel.Information,
                        "Scraper Completed",
                        $"{scraper.GetType().Name} returned {tenders.Count} tenders.",
                        cancellationToken: cancellationToken);
                }

                var etendersReleases = await _etendersClient.GetAllReleasesAsync(
                    DateTime.UtcNow.Date.AddDays(-1),
                    DateTime.UtcNow.Date,
                    cancellationToken: cancellationToken);
               
                var constructionReleases =
                    _etendersConstructionFilter.Filter(etendersReleases);

                foreach (var release in constructionReleases)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var tender = _etendersTenderMapper.Map(
                        release,
                        sync.Id);

                    scrapedTenders.Add(tender);
                }

                sync.TotalRetrieved = scrapedTenders.Count;

                var crmTenders = await _dataverseService
                    .GetAdvertisedTendersAsync(cancellationToken);

                var crmTenderNumbers = crmTenders
                    .Where(x => !string.IsNullOrWhiteSpace(x.TenderNumber))
                    .Select(x => x.TenderNumber.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var stream3Tenders = scrapedTenders
                    .Where(x =>
                        x.AwardedDate.HasValue &&
                        x.AwardValue.HasValue)
                    .ToList();

                List<ContractModel> crmContracts = [];

                if (stream3Tenders.Any())
                {
                    var earliestAwardDate = stream3Tenders
                        .Min(x => x.AwardedDate!.Value);

                    crmContracts = await _dataverseService
                        .GetAwardedContractsAsync(
                            earliestAwardDate,
                            cancellationToken);
                }

                var compliant = 0;
                var nonCompliant = 0;
                var casesCreated = 0;

                foreach (var tender in scrapedTenders)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (string.IsNullOrWhiteSpace(tender.TenderNumber))
                        continue;

                    // Check for duplicate in our own DB
                    var existingTender = await _tenderRepository
                        .GetByTenderNumberAsync(tender.TenderNumber, cancellationToken);
                    if (existingTender != null)
                    {
                        await LogAsync(
                            sync.Id,
                            SyncLogType.Duplicate,
                            SyncLogLevel.Warning,
                            "Duplicate Tender",
                            "Tender has already been synchronized previously.",
                            tender.TenderNumber,
                            cancellationToken);
                        continue;
                    }

                    // Save tender
                    tender.TenderSyncId = sync.Id;
                    await _tenderRepository.AddAsync(tender, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken); // get Id

                    await LogAsync(
                        sync.Id,
                        SyncLogType.TenderImported,
                        SyncLogLevel.Information,
                        "Tender Imported",
                        "Tender imported successfully.",
                        tender.TenderNumber,
                        cancellationToken);

                    // ---- Process Compliance ----
                    Guid? createdCaseId = null;
                    try
                    {
                        createdCaseId = await _complianceProcessingService.ProcessTenderAsync(
                            tender,
                            crmTenderNumbers,
                            crmContracts,
                            sync.Id,
                            executionUserId,
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await LogAsync(
                            sync.Id,
                            SyncLogType.Error,
                            SyncLogLevel.Error,
                            "Compliance Processing Failed",
                            $"Error processing tender {tender.TenderNumber}: {ex.Message}",
                            tender.TenderNumber,
                            cancellationToken);
                        sync.ErrorCount++;
                    }

                    if (createdCaseId.HasValue)
                    {
                        nonCompliant++;
                        casesCreated++;
                    }
                    else
                    {
                        compliant++;
                    }
                }

                sync.CompletedOn = DateTime.UtcNow;
                sync.Status = SyncStatus.Completed;
                sync.TotalCompliant = compliant;
                sync.TotalNonCompliant = nonCompliant;
                sync.CasesCreated = casesCreated;

                await LogAsync(
                    sync.Id,
                    SyncLogType.Information,
                    SyncLogLevel.Information,
                    "Synchronization Completed",
                    $"Retrieved {sync.TotalRetrieved}, Created {casesCreated} compliance cases.",
                    cancellationToken: cancellationToken);

                await _tenderSyncRepository.UpdateAsync(sync, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                sync.CompletedOn = DateTime.UtcNow;
                sync.Status = SyncStatus.Failed;
                sync.ErrorCount++;

                await LogAsync(
                    sync.Id,
                    SyncLogType.Error,
                    SyncLogLevel.Error,
                    "Connection to iTender Failed",
                    ex.Message,
                    cancellationToken: cancellationToken);

                await _tenderSyncRepository.UpdateAsync(sync, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return;
            }
            catch (Exception ex)
            {
                sync.CompletedOn = DateTime.UtcNow;
                sync.Status = SyncStatus.Failed;
                sync.ErrorCount++;

                await LogAsync(
                    sync.Id,
                    SyncLogType.Error,
                    SyncLogLevel.Error,
                    "Synchronization Failed",
                    ex.ToString(),
                    cancellationToken: cancellationToken);

                await _tenderSyncRepository.UpdateAsync(sync, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return;
            }
        }

        private async Task LogAsync(
            Guid syncId,
            SyncLogType type,
            SyncLogLevel level,
            string title,
            string message,
            string? tenderNumber = null,
            CancellationToken cancellationToken = default)
        {
            await _syncLogRepository.AddAsync(new TenderSyncLog
            {
                TenderSyncId = syncId,
                Type = type,
                Level = level,
                Title = title,
                Message = message,
                TenderNumber = tenderNumber
            }, cancellationToken);
        }

        private Guid? GetExecutionUserId(bool isManual)
        {
            if (isManual)
                return _currentUser.UserId;

            return _configuration.GetValue<Guid?>(
                "Synchronization:SystemUserId");
        }
    }
}
