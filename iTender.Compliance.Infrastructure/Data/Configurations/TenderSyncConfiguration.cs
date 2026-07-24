using iTender.Compliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iTender.Compliance.Infrastructure.Data.Configurations
{
    public class TenderSyncConfiguration
    : IEntityTypeConfiguration<TenderSync>
    {
        public void Configure(EntityTypeBuilder<TenderSync> builder)
        {
            builder.ToTable("TenderSyncs");

            builder.HasKey(x => x.Id);

            builder.HasMany(x => x.Tenders)
                .WithOne(x => x.TenderSync)
                .HasForeignKey(x => x.TenderSyncId);
        }
    }
}
