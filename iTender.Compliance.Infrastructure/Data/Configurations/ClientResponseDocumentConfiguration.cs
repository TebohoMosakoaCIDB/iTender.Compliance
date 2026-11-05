using iTender.Compliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iTender.Compliance.Infrastructure.Data.Configurations
{
    public class ClientResponseDocumentConfiguration
        : IEntityTypeConfiguration<ClientResponseDocument>
    {
        public void Configure(EntityTypeBuilder<ClientResponseDocument> builder)
        {
            builder.ToTable("ClientResponseDocuments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FileName)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(x => x.StoragePath)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(x => x.ContentType)
                .HasMaxLength(150)
                .IsRequired();

            builder.HasOne(x => x.ClientResponse)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.ClientResponseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.ClientResponseId);
        }
    }
}