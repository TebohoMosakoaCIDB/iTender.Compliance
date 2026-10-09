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
        private readonly IComplianceFindingRepository _findingRepository;
        private readonly ITenderRepository _tenderRepository;
        private readonly IEtendersScraperClient _etendersScraperClient;
        private readonly ScrapedTenderMapper _scrapedTenderMapper;
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
        private readonly IClassOfWorksResolver _classOfWorksResolver;
        private readonly IAutoAssignmentService _autoAssignmentService;
        private readonly ISystemSettingRepository _systemSettingRepository;
        private readonly IComplianceProcessingService _complianceProcessingService;
        private readonly IEtendersClient _etendersClient;
        private readonly EtendersConstructionFilter _etendersConstructionFilter;
        private readonly EtendersTenderMapper _etendersTenderMapper;
        public SynchronizationService(
            IComplianceFindingRepository findingRepository,
            IEtendersScraperClient etendersScraperClient,
            IEnumerable<IScraperService> scrapers,
            ScrapedTenderMapper scrapedTenderMapper,
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
            IClassOfWorksResolver classOfWorksResolver,
            EtendersTenderMapper etendersTenderMapper)
        {
            _scrapers = scrapers;
            _currentUser = currentUser;
            _findingRepository = findingRepository;
            _verificationService = verificationService;
            _etendersScraperClient = etendersScraperClient;
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
            _classOfWorksResolver = classOfWorksResolver;
            _etendersTenderMapper = etendersTenderMapper;
            _scrapedTenderMapper = scrapedTenderMapper;
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
                // Get eTenders tenders - API first, scraper fallback
                // ---------------------------------------------------------
                var apiSucceeded = false;

                try
                {
                    var releases = await _etendersClient.GetAllReleasesAsync(
                        DateTime.UtcNow.Date.AddMonths(-1),
                        DateTime.UtcNow.Date,
                        cancellationToken: cancellationToken);

                    var constructionReleases = _etendersConstructionFilter.Filter(releases);

                    foreach (var release in constructionReleases)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var tender = _etendersTenderMapper.Map(release, sync.Id);
                        scrapedTenders.Add(tender);
                    }

                    apiSucceeded = true;

                    await LogAsync(
                        sync.Id,
                        SyncLogType.Information,
                        SyncLogLevel.Information,
                        "eTenders API Retrieved",
                        $"API returned {scrapedTenders.Count} construction tenders.",
                        cancellationToken: cancellationToken);
                }
                catch (Exception apiEx)
                {
                    await LogAsync(
                        sync.Id,
                        SyncLogType.Information,
                        SyncLogLevel.Warning,
                        "eTenders API Failed - Trying Scraper",
                        $"API error: {apiEx.Message}. Falling back to scraper.",
                        cancellationToken: cancellationToken);
                }

                if (!apiSucceeded)
                {
                    try
                    {
                        var scraped = await _etendersScraperClient
                            .ScrapeAsync(status: 1, cancellationToken);

                        if (scraped is not null && scraped.Tenders.Count > 0)
                        {
                            foreach (var dto in scraped.Tenders)
                            {
                                cancellationToken.ThrowIfCancellationRequested();

                                var tender = _scrapedTenderMapper.Map(dto, sync.Id);
                                if (tender is null)
                                    continue;

                                // Only include tenders that pass the construction filter.
                                if (!IsConstructionTender(dto.Category, dto.TenderNumber, dto.Title))
                                    continue;

                                // Resolve class of works from the tender description/title.
                                if (string.IsNullOrWhiteSpace(tender.ClassOfWorks))
                                {
                                    tender.ClassOfWorks = await _classOfWorksResolver
                                        .ResolveAsync(tender, cancellationToken);
                                }

                                scrapedTenders.Add(tender);
                            }

                            await LogAsync(
                                sync.Id,
                                SyncLogType.Information,
                                SyncLogLevel.Warning,
                                "Scraper Fallback Succeeded",
                                $"Scraper returned {scraped.Count} tenders; {scrapedTenders.Count} construction tenders kept.",
                                cancellationToken: cancellationToken);
                        }
                        else
                        {
                            await LogAsync(
                                sync.Id,
                                SyncLogType.Error,
                                SyncLogLevel.Error,
                                "Scraper Fallback Failed",
                                "Scraper returned no tenders. Sync will continue with zero eTenders results.",
                                cancellationToken: cancellationToken);
                        }
                    }
                    catch (Exception scraperEx)
                    {
                        await LogAsync(
                            sync.Id,
                            SyncLogType.Error,
                            SyncLogLevel.Error,
                            "Scraper Fallback Error",
                            $"Scraper failed: {scraperEx.Message}.",
                            cancellationToken: cancellationToken);
                    }
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

                // NEW: keyed by normalized employer + normalized title, so we can spot
                // "same tender advertised under a different number" across the two sources.
                var crmTendersByScope = crmTenders
                    .Where(x => !string.IsNullOrWhiteSpace(x.TenderNumber)
                             && !string.IsNullOrWhiteSpace(x.EmployerName)
                             && !string.IsNullOrWhiteSpace(x.Title))
                    .GroupBy(x => TenderMatchKeyBuilder.Build(x.EmployerName, x.Title))
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

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
                var skipped = 0;

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
                    tender.NormalizedTitle = Domain.Utilities.TenderTitleNormalizer.Normalize(tender.Title);
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
                    // D05: Detect potential duplicates and group for CO.
                    // NOTE: we still create the compliance case below so the
                    // tender appears in the CO's work baskets per BR-8.1.
                    // The duplicate group is an additional signal for the CO.
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

                        var duplicateGroup = new DuplicateGroup
                        {
                            Id = Guid.NewGuid(),
                            TenderSyncId = sync.Id,
                            Status = DuplicateStatus.PendingCOVerification,
                            Candidates = candidates
                        };

                        await _duplicateGroupRepository.AddAsync(duplicateGroup, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        await LogAsync(
                            sync.Id,
                            SyncLogType.Duplicate,
                            SyncLogLevel.Warning,
                            "Potential Duplicate Detected",
                            $"Tender was grouped with {candidates.Count - 1} potential duplicate(s) for CO verification. " +
                            $"Compliance processing will still run so the tender appears in the work baskets.",
                            tender.TenderNumber,
                            cancellationToken);

                        // NO continue — fall through to ProcessTenderAsync.
                    }

                    // -----------------------------------------------------
                    // Process compliance
                    // -----------------------------------------------------
                    Guid? createdCaseId = null;

                    try
                    {
                        createdCaseId = await _complianceProcessingService.ProcessTenderAsync(
                            tender,
                            crmTenderNumbers,
                            crmTendersByScope,
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
                            "Tender Compliance Check Failed",
                            $"The tender was imported, but its compliance check could not be completed. Error: {ex.Message}",
                            tender.TenderNumber,
                            cancellationToken);

                        sync.ErrorCount++;
                    }

                    if (createdCaseId.HasValue)
                    {
                        casesCreated++;

                        // Count findings via a direct query — do not rely on navigation loading.
                        var findingCount = await _findingRepository
                            .CountByCaseIdAsync(createdCaseId.Value, cancellationToken);

                        if (findingCount > 0)
                            nonCompliant++;
                        else
                            compliant++;

                        // Stream 3 verification for awarded tenders.
                        if (tender.AwardedDate.HasValue && tender.AwardValue.HasValue)
                        {
                            try
                            {
                                var createdCase = await _complianceCaseRepository
                                    .GetByIdAsync(createdCaseId.Value, cancellationToken);

                                if (createdCase is not null)
                                {
                                    await _verificationService
                                        .RunAutomatedVerificationAsync(createdCase, cancellationToken);
                                }
                            }
                            catch (Exception ex)
                            {
                                var inner = ex;
                                var detail = ex.Message;

                                while (inner.InnerException != null)
                                {
                                    inner = inner.InnerException;
                                    detail += $" -> {inner.Message}";
                                }

                                await LogAsync(
                                    sync.Id,
                                    SyncLogType.Error,
                                    SyncLogLevel.Error,
                                    "Tender Compliance Check Failed",
                                    $"The tender was imported, but its compliance check could not be completed. " +
                                    $"Error: {detail}",
                                    tender.TenderNumber,
                                    cancellationToken);

                                sync.ErrorCount++;
                            }
                        }
                    }
                    else
                    {
                        // ProcessTenderAsync only returns null for non-construction tenders.
                        skipped++;
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
                    $"Retrieved: {sync.TotalRetrieved}, " +
                    $"Cases created: {casesCreated} " +
                    $"(compliant: {compliant}, non-compliant: {nonCompliant}), " +
                    $"Skipped: {skipped}, " +
                    $"Errors: {sync.ErrorCount}.",
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

        private static readonly HashSet<string> ConstructionCategories = new(StringComparer.OrdinalIgnoreCase)
        {
            // Group A
            "Civil engineering",
            "Construction",
            "Construction of buildings",
            "Services: Civil",
            "Specialised construction activities",
            "Services: Building",
            "Services: Electrical",
            "Electricity, gas, steam and air conditioning",

            // Group B
            "Services to buildings and landscape activities",
            "Water supply; sewerage, waste management and remediation activities",
            "Water collection, treatment and supply",
            "Mining and quarrying",
            "Repair and installation of machinery and equipment",
            "Remediation activities and other waste management services",
            "Land transport and transport via pipelines",
            "Sewerage",
            "Waste collection, treatment and disposal activities; materials recovery",
        };

        private static readonly HashSet<string> BroadConstructionCategories = new(StringComparer.OrdinalIgnoreCase)
        {
            "Services to buildings and landscape activities",
            "Water supply; sewerage, waste management and remediation activities",
            "Water collection, treatment and supply",
            "Mining and quarrying",
            "Repair and installation of machinery and equipment",
            "Remediation activities and other waste management services",
            "Land transport and transport via pipelines",
            "Sewerage",
            "Waste collection, treatment and disposal activities; materials recovery",
            "Construction",
            "Specialised construction activities",
        };

        private static bool IsConstructionTender(string? category, string? title, string? description)
        {
            if (string.IsNullOrWhiteSpace(category))
                return false;

            var trimmed = category.Trim();

            // Narrow construction categories: accept directly.
            if (ConstructionCategories.Contains(trimmed) && !BroadConstructionCategories.Contains(trimmed))
                return true;

            // Broad construction categories: require a construction keyword in the text.
            if (BroadConstructionCategories.Contains(trimmed))
            {
                var haystack = $"{title} {description}".Trim();
                return CategoryMappingService.ClassKeywords.Values
                    .SelectMany(k => k)
                    .Any(k => KeywordMatcher.ContainsKeyword(haystack, k));
            }

            return false;
        }
    }
}
