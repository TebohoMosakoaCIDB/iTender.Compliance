using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
 
namespace iTender.Compliance.Infrastructure.Data
{
    public class ComplianceDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ComplianceDbContext(
            DbContextOptions<ComplianceDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tender> Tenders => Set<Tender>();

        public DbSet<ComplianceCase> ComplianceCases => Set<ComplianceCase>();

        public DbSet<Agent> Agents => Set<Agent>();

        public DbSet<CaseLetter> CaseLetters => Set<CaseLetter>();

        public DbSet<CaseNote> Notes => Set<CaseNote>();

        public DbSet<TenderSync> TenderSyncs => Set<TenderSync>();

        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

        public DbSet<TenderSyncLog> SyncLogs => Set<TenderSyncLog>();

        public DbSet<Notification> Notifications => Set<Notification>();

        public DbSet<CorrespondenceTemplateModel> CorrespondenceTemplates => Set<CorrespondenceTemplateModel>();

        public DbSet<SigningRequest> SigningRequests => Set<SigningRequest>();

        public DbSet<AGSAReferral> AGSAReferrals => Set<AGSAReferral>();

        public DbSet<CaseObjection> CaseObjections => Set<CaseObjection>();

        public DbSet<ComplianceAction> ComplianceActions => Set<ComplianceAction>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(ComplianceDbContext).Assembly);
        }
    }
}