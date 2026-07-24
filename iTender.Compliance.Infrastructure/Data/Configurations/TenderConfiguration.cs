using iTender.Compliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iTender.Compliance.Infrastructure.Data.Configurations
{
    public class TenderConfiguration : IEntityTypeConfiguration<Tender>
    {
        public void Configure(EntityTypeBuilder<Tender> builder)
        {
            builder.ToTable("Tenders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TenderNumber)
                .HasMaxLength(500)
                .IsRequired();

            builder.HasIndex(x => x.TenderNumber)
                .IsUnique();

            builder.Property(x => x.Title)
                .HasMaxLength(5000)
                .IsRequired();

            builder.Property(x => x.TenderUrl)
                .HasMaxLength(4000)
                .IsRequired();

            builder.HasOne(x => x.TenderSync)
                .WithMany(x => x.Tenders)
                .HasForeignKey(x => x.TenderSyncId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ComplianceCase)
                .WithOne(x => x.Tender)
                .HasForeignKey<ComplianceCase>(x => x.TenderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
