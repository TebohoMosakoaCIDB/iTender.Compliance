using iTender.Compliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iTender.Compliance.Infrastructure.Data.Configurations
{
    public class DuplicateGroupConfiguration : IEntityTypeConfiguration<DuplicateGroup>
    {
        public void Configure(EntityTypeBuilder<DuplicateGroup> builder)
        {
            builder.ToTable("DuplicateGroups");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(x => x.CoComment)
                   .HasMaxLength(2000);

            builder.HasOne(x => x.TenderSync)
                   .WithMany()
                   .HasForeignKey(x => x.TenderSyncId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Candidates)
                   .WithOne(x => x.DuplicateGroup)
                   .HasForeignKey(x => x.DuplicateGroupId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.TenderSyncId);
        }
    }
}
