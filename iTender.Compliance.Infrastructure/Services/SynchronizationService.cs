using iTender.Application.DTOs;
using iTender.Compliance.Application.Filters;
using iTender.Compliance.Application.Interfaces;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Scrapers;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Domain.Utilities;
using iTender.Compliance.Infrastructure.Mappers;
using Microsoft.Extensions.Configuration;

namespace iTender.Compliance.Infrastructure.Services
{
    public class SynchronizationService : ISynchronizationService
    {
        private readonly ITenderRepository _tenderRepository;
        private readonly IVerificationService _verificationService;
        private readonly IDuplicateGroupRepository _duplicateGroupRepository;
        private readonly IDuplicateDetectionService _duplicateDetectionService;
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
            IVerificationService verificationService,
            IDuplicateGroupRepository duplicateGroupRepository,
            IDuplicateDetectionService duplicateDetectionService,
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
            _verificationService = verificationService;
            _tenderRepository = tenderRepository;
            _duplicateDetectionService = duplicateDetectionService;
            _duplicateGroupRepository = duplicateGroupRepository;
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

        public async Task SynchronizeAsync(
    bool isManual,
    CancellationToken cancellationToken = default)
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

                // ---------------------------------------------------------
                // Scrapers
                // ---------------------------------------------------------
                // foreach (var scraper in _scrapers)
                // {
                //     var tenders = await scraper.ScrapeAsync(cancellationToken);
                //
                //     scrapedTenders.AddRange(tenders);
                //
                //     await LogAsync(
                //         sync.Id,
                //         SyncLogType.Information,
                //         SyncLogLevel.Information,
                //         "Scraper Completed",
                //         $"{scraper.GetType().Name} returned {tenders.Count} tenders.",
                //         cancellationToken: cancellationToken);
                // }

                // ---------------------------------------------------------
                // Get eTenders releases
                // ---------------------------------------------------------
                var etendersReleases = await _etendersClient.GetAllReleasesAsync(
                    DateTime.UtcNow.Date.AddMonths(-1),
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

                await LogAsync(
                    sync.Id,
                    SyncLogType.Information,
                    SyncLogLevel.Information,
                    "Tender Information Retrieved",
                    $"{sync.TotalRetrieved} relevant tenders were retrieved for processing.",
                    cancellationToken: cancellationToken);

                // ---------------------------------------------------------
                // Get advertised tenders from CRM
                // ---------------------------------------------------------
                var crmTenders = await _dataverseService
                    .GetAdvertisedTendersAsync(cancellationToken);

                var crmTenderNumbers = crmTenders
                    .Where(x => !string.IsNullOrWhiteSpace(x.TenderNumber))
                    .Select(x => x.TenderNumber.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                // ---------------------------------------------------------
                // Identify Stream 3 tenders
                // ---------------------------------------------------------
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

                // ---------------------------------------------------------
                // Process tenders
                // ---------------------------------------------------------
                var compliant = 0;
                var nonCompliant = 0;
                var casesCreated = 0;

                foreach (var tender in scrapedTenders)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (string.IsNullOrWhiteSpace(tender.TenderNumber))
                        continue;

                    // -----------------------------------------------------
                    // Exact duplicate - skip as before
                    // -----------------------------------------------------
                    var existingTender = await _tenderRepository
                        .GetByTenderNumberAsync(tender.TenderNumber, cancellationToken);

                    if (existingTender != null)
                    {
                        await LogAsync(
                            sync.Id,
                            SyncLogType.Duplicate,
                            SyncLogLevel.Warning,
                            "Tender Already Processed",
                            "This tender has already been synchronized and was skipped.",
                            tender.TenderNumber,
                            cancellationToken);

                        continue;
                    }

                    // -----------------------------------------------------
                    // Save tender
                    // -----------------------------------------------------
                    tender.TenderSyncId = sync.Id;
                    tender.NormalizedTitle = TenderTitleNormalizer.Normalize(tender.Title);
                    tender.NormalizedEmployerName = EmployerNameNormalizer.Normalize(tender.EmployerName);

                    await _tenderRepository.AddAsync(tender, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    await LogAsync(
                        sync.Id,
                        SyncLogType.TenderImported,
                        SyncLogLevel.Information,
                        "Tender Imported",
                        "Tender information was imported successfully.",
                        tender.TenderNumber,
                        cancellationToken);

                    // -----------------------------------------------------
                    // D05: Detect potential duplicates and group for CO
                    // -----------------------------------------------------
                    var detectedDuplicates = await _duplicateDetectionService
    .FindPotentialDuplicatesAsync(tender, cancellationToken);

                    if (detectedDuplicates.Count > 0)
                    {
                        var candidates = detectedDuplicates
                            .Append(new DuplicateCandidate
                            {
                                Id = Guid.NewGuid(),
                                TenderId = tender.Id,
                                MatchType = DuplicateMatchType.NormalizedTitle,
                                ConfidenceScore = 1.0,
                                MatchedOn = "Triggering tender"
                            })
                            .ToList();

                        var group = new DuplicateGroup
                        {
                            Id = Guid.NewGuid(),
                            TenderSyncId = sync.Id,
                            Status = DuplicateStatus.PendingCOVerification,
                            Candidates = candidates
                        };

                        await _duplicateGroupRepository.AddAsync(group, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        await LogAsync(
                            sync.Id,
                            SyncLogType.Duplicate,
                            SyncLogLevel.Warning,
                            "Potential Duplicate Detected",
                            $"Tender was grouped with {candidates.Count - 1} potential duplicate(s) for CO verification.",
                            tender.TenderNumber,
                            cancellationToken);

                        continue;
                    }

                    // -----------------------------------------------------
                    // Process compliance (unchanged)
                    // -----------------------------------------------------
                    Guid? createdCaseId = null;

                    try
                    {
                        createdCaseId = await _complianceProcessingService.ProcessTenderAsync(
                            tender, crmTenderNumbers, crmContracts, sync.Id, executionUserId, cancellationToken);
                    }
                    catch (Exception)
                    {
                        await LogAsync(
                            sync.Id,
                            SyncLogType.Error,
                            SyncLogLevel.Error,
                            "Tender Compliance Check Failed",
                            "The tender was imported, but its compliance check could not be completed.",
                            tender.TenderNumber,
                            cancellationToken);

                        sync.ErrorCount++;
                    }

                    if (createdCaseId.HasValue)
                    {
                        nonCompliant++;
                        casesCreated++;

                        if (tender.AwardedDate.HasValue && tender.AwardValue.HasValue)
                        {
                            try
                            {
                                var complianceCase = await _complianceCaseRepository
                                    .GetByIdAsync(createdCaseId.Value, cancellationToken);

                                if (complianceCase is not null)
                                {
                                    await _verificationService
                                        .RunAutomatedVerificationAsync(complianceCase, cancellationToken);
                                }
                            }
                            catch (Exception ex)
                            {
                                await LogAsync(
                                    sync.Id,
                                    SyncLogType.Error,
                                    SyncLogLevel.Error,
                                    "Verification Failed",
                                    "Case was created but automated verification could not be completed.",
                                    tender.TenderNumber,
                                    cancellationToken);

                                sync.ErrorCount++;
                            }
                        }
                    }
                    else
                    {
                        compliant++;
                    }
                }

                // ---------------------------------------------------------
                // Complete synchronization
                // ---------------------------------------------------------
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
                    $"Synchronization completed successfully. " +
                    $"{sync.TotalRetrieved} tenders were retrieved and " +
                    $"{casesCreated} compliance cases were created.",
                    cancellationToken: cancellationToken);

                await _tenderSyncRepository.UpdateAsync(
                    sync,
                    cancellationToken);

                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);
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
                    "Unable to Retrieve Tender Information",
                    "We couldn't retrieve the latest tender information from the required service. Please try again later.",
                    cancellationToken: cancellationToken);

                await _tenderSyncRepository.UpdateAsync(
                    sync,
                    cancellationToken);

                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);

                return;
            }
            catch (InvalidOperationException ex)
            {
                sync.CompletedOn = DateTime.UtcNow;
                sync.Status = SyncStatus.Failed;
                sync.ErrorCount++;

                await LogAsync(
                    sync.Id,
                    SyncLogType.Error,
                    SyncLogLevel.Error,
                    "Unable to Retrieve Tender Information",
                    ex.Message,
                    cancellationToken: cancellationToken);

                await _tenderSyncRepository.UpdateAsync(
                    sync,
                    cancellationToken);

                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);

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
                    "We couldn't complete the synchronization. Please try again later. If the problem continues, contact the system administrator.",
                    cancellationToken: cancellationToken);

                await _tenderSyncRepository.UpdateAsync(
                    sync,
                    cancellationToken);

                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);

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
