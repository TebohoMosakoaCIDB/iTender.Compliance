using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Scrapers;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Application.Providers;
using iTender.Compliance.Infrastructure.Data;
using iTender.Compliance.Infrastructure.Models;
using iTender.Compliance.Infrastructure.Providers;
using iTender.Compliance.Infrastructure.Repositories;
using iTender.Compliance.Infrastructure.Scrapers;
using iTender.Compliance.Infrastructure.Services;
using iTender.Compliance.Infrastructure.Services.BackgroundServices;
using iTender.Compliance.Infrastructure.Services.SigningHub;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QuestPDF.Infrastructure;
using System.Net.Http.Headers;

namespace iTender.Compliance.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SigningHubOptions>(
                configuration.GetSection(SigningHubOptions.SectionName));

            services.AddHttpClient<ISigningHubApiProvider, SigningHubApiProvider>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<SigningHubOptions>>().Value;

                client.BaseAddress = new Uri(options.BaseUrl);

                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            });

            QuestPDF.Settings.License = LicenseType.Evaluation;
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddSignalRCore();

            //Strategies
            services.AddScoped<LeastWorkloadStrategy>();
            services.AddScoped<PriorityBasedAssignmentStrategy>();
            services.AddScoped<RandomStrategy>();
            services.AddScoped<RoundRobinStrategy>();

            //Repos
            services.AddScoped<ITenderRepository, TenderRepository>();
            services.AddScoped<IComplianceCaseRepository, ComplianceCaseRepository>();
            services.AddScoped<ITenderSyncRepository, TenderSyncRepository>();
            services.AddScoped<ICaseLetterRepository, CaseLetterRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IAgentRepository, AgentRepository>();
            services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
            services.AddScoped<ICaseNoteRepository, CaseNoteRepository>();
            services.AddScoped<ITenderSyncLogRepository, TenderSyncLogRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<ICorrespondenceTemplateRepository, CorrespondenceTemplateRepository>();
            services.AddScoped<ISigningRequestRepository, SigningRequestRepository>();
            services.AddScoped<IComplianceFindingRepository, ComplianceFindingRepository>();
            services.AddScoped<IComplianceActionRepository, ComplianceActionRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();

            //Services
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IComplianceService, ComplianceService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<ISynchronizationService, SynchronizationService>();
            services.AddScoped<IReportingService, ReportingService>();
            services.AddScoped<IDataverseService, DataverseService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IUserService, UserService>();
            //services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<ISystemSettingService, SystemSettingService>();
            services.AddScoped<ICaseAuthorizationService, CaseAuthorizationService>();

            services.AddHostedService<TenderSyncBackgroundService>();
            services.AddHostedService<AutoAssignmentBackgroundService>();
            services.AddHostedService<RoPComplianceBackgroundService>();

            services.AddScoped<IAutoAssignmentService, AutoAssignmentService>();
            services.AddScoped<ITenderService, TenderService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IReportExcelService, ReportExcelService>();
            services.AddScoped<IReportPdfService, ReportPdfService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ICorrespondenceTemplateService, CorrespondenceTemplateService>();
            services.AddScoped<ISigningRequestService, SigningRequestService>();
            services.AddScoped<IComplianceProcessingService, ComplianceProcessingService>();
            services.AddScoped<IWorkClassificationValidator, WorkClassificationValidator>();
            services.AddScoped<ILetterNumberGenerator, LetterNumberGenerator>();
            services.AddScoped<IComplianceFollowUpService, ComplianceFollowUpService>();
            services.AddScoped<IRoPComplianceService, RoPComplianceService>();
            services.AddScoped<ICorrespondenceService, CorrespondenceService>();



            services.AddScoped<IProfileService, ProfileService>();
            services.AddHttpClient("TenderFlow", client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            });

            //services.AddScoped<IScraperService, TenderFlowScraper>();

            // HTTP client with named or typed client
            services.Configure<ETenderApiOptions>(configuration.GetSection("ETenderApi"));
            services.Configure<OpenAIOptions>(configuration.GetSection("OpenAI"));
            services.AddHttpClient<ETenderApiScraper>();
            services.AddScoped<IScraperService, ETenderApiScraper>();
            services.AddScoped<ICategoryMappingService, CategoryMappingService>();

            services.AddScoped<
                    ITenderDiscoveryAgent,
                    TenderDiscoveryAgent>();

            return services;
        }
    }
}
