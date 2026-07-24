using iTender.Compliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iTender.Compliance.Infrastructure.Data.Configurations
{
    public class ComplianceCaseConfiguration : IEntityTypeConfiguration<ComplianceCase>
    {
        public void Configure(EntityTypeBuilder<ComplianceCase> builder)
        {
            builder.ToTable("ComplianceCases");

            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Agent)
                .WithMany(x => x.ComplianceCases)
                .HasForeignKey(x => x.AgentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.CaseLetters)
                .WithOne(x => x.ComplianceCase)
                .HasForeignKey(x => x.ComplianceCaseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
