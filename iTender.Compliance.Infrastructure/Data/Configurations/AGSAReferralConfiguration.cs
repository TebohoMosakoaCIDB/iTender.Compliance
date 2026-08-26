using iTender.Compliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iTender.Compliance.Infrastructure.Data.Configurations
{
    public class AGSAReferralConfiguration : IEntityTypeConfiguration<AGSAReferral>
    {
        public void Configure(EntityTypeBuilder<AGSAReferral> builder)
        {
            builder.ToTable("AGSAReferrals");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReferralNumber)
                .HasMaxLength(50);

            builder.HasIndex(x => x.ComplianceCaseId)
                .IsUnique();

            builder.HasOne(x => x.ComplianceCase)
                .WithOne(x => x.AGSAReferral)
                .HasForeignKey<AGSAReferral>(x => x.ComplianceCaseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
