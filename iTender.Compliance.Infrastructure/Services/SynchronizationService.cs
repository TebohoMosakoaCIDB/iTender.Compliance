using iTender.Compliance.Application.Interfaces;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Scrapers;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
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
        private readonly IEnumerable<IScraperService> _scrapers;
        private readonly IDataverseService _dataverseService;
        private readonly ICurrentUserService _currentUser;
        private readonly ITenderSyncLogRepository _syncLogRepository;
        private readonly IConfiguration _configuration;
        private readonly IAutoAssignmentService _autoAssignmentService;
        private readonly ISystemSettingRepository _systemSettingRepository;

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
            IAutoAssignmentService autoAssignmentService)
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

                    await LogAsync(
                        sync.Id,
                        SyncLogType.Information,
                        SyncLogLevel.Information,
                        "Scraper Completed",
                        $"{scraper.GetType().Name} returned {tenders.Count} tenders.",
                        cancellationToken: cancellationToken);
                }

                sync.TotalRetrieved = scrapedTenders.Count;

                var crmTenders = await _dataverseService
                    .GetAdvertisedTendersAsync(cancellationToken);

                var crmTenderNumbers = crmTenders
                    .Where(x => !string.IsNullOrWhiteSpace(x.TenderNumber))
                    .Select(x => x.TenderNumber.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var compliant = 0;
                var nonCompliant = 0;
                var casesCreated = 0;

                foreach (var tender in scrapedTenders)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (string.IsNullOrWhiteSpace(tender.TenderNumber))
                        continue;

                    if (crmTenderNumbers.Contains(tender.TenderNumber.Trim()))
                    {
                        compliant++;

                        await LogAsync(
                            sync.Id,
                            SyncLogType.TenderSkipped,
                            SyncLogLevel.Information,
                            "Tender Exists",
                            "Tender already exists in Dataverse.",
                            tender.TenderNumber,
                            cancellationToken);

                        continue;
                    }

                    var existingTender = await _tenderRepository
                        .GetByTenderNumberAsync(
                            tender.TenderNumber,
                            cancellationToken);

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

                    tender.TenderSyncId = sync.Id;

                    await _tenderRepository.AddAsync(
                        tender,
                        cancellationToken);

                    await LogAsync(
                        sync.Id,
                        SyncLogType.TenderImported,
                        SyncLogLevel.Information,
                        "Tender Imported",
                        "Tender imported successfully.",
                        tender.TenderNumber,
                        cancellationToken);

                    var complianceCase = new ComplianceCase
                    {
                        TenderId = tender.Id,
                        Status = CaseStatus.New,
                        Priority = CasePriority.Normal
                    };
                    var settings = await _systemSettingRepository.GetAsync(cancellationToken);
                    var agent = await _autoAssignmentService.SelectAgentAsync(
                        complianceCase,
                        settings.DistributionMethod,
                        cancellationToken);

                    if (agent != null)
                    {
                        complianceCase.AgentId = agent.Id;
                        complianceCase.AssignedOn = DateTime.UtcNow;
                        complianceCase.Status = CaseStatus.Assigned;
                    }

                    await _complianceCaseRepository.AddAsync(
                        complianceCase,
                        cancellationToken);

                    await _auditService.LogAsync(
                        AuditAction.Created,
                        AuditEntity.ComplianceCase,
                        complianceCase.Id,
                        $"Compliance case created for tender '{tender.TenderNumber}'.",
                        executionUserId,
                        cancellationToken);

                    await LogAsync(
                        sync.Id,
                        SyncLogType.CaseCreated,
                        SyncLogLevel.Information,
                        "Compliance Case Created",
                        "Compliance case created successfully.",
                        tender.TenderNumber,
                        cancellationToken);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    nonCompliant++;
                    casesCreated++;
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
                    "Dataverse Connection Failed",
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
