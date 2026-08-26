using iTender.Compliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iTender.Compliance.Infrastructure.Data.Configurations
{
    public class CaseObjectionConfiguration : IEntityTypeConfiguration<CaseObjection>
    {
        public void Configure(EntityTypeBuilder<CaseObjection> builder)
        {
            builder.ToTable("CaseObjections");

            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.ComplianceCase)
                .WithMany(x => x.Objections)
                .HasForeignKey(x => x.ComplianceCaseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.CaseLetter)
                .WithMany()
                .HasForeignKey(x => x.CaseLetterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}