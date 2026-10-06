using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iTender.Compliance.Infrastructure.Data.Configurations
{
    public class ClientGroupCaseConfiguration : IEntityTypeConfiguration<ClientGroupCase>
    {
        public void Configure(EntityTypeBuilder<ClientGroupCase> builder)
        {
            builder.ToTable("ClientGroupCases");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.ClientGroup)
                   .WithMany(g => g.Cases)
                   .HasForeignKey(x => x.ClientGroupId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ComplianceCase)
                   .WithMany(c => c.ClientGroups)
                   .HasForeignKey(x => x.ComplianceCaseId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.ClientGroupId, x.ComplianceCaseId })
                   .IsUnique();

            builder.HasIndex(x => x.ComplianceCaseId);
        }
    }
}