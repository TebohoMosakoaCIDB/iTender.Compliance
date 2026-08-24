using iTender.Compliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iTender.Compliance.Infrastructure.Data.Configurations
{
    public class SigningRequestConfiguration : IEntityTypeConfiguration<SigningRequest>
    {
        public void Configure(EntityTypeBuilder<SigningRequest> builder)
        {
            builder.ToTable("SigningRequests");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.DocumentId)
                .HasMaxLength(200);

            builder.Property(x => x.OriginalDocumentPath)
                .HasMaxLength(500);

            builder.Property(x => x.SignedDocumentPath)
                .HasMaxLength(500);

            builder.Property(x => x.SignerName)
                .HasMaxLength(200);

            builder.Property(x => x.SignerEmail)
                .HasMaxLength(250);

            builder.Property(x => x.FailureReason)
                .HasMaxLength(1000);

            builder.HasOne(x => x.CaseLetter)
                .WithOne(x => x.SigningRequest)
                .HasForeignKey<SigningRequest>(x => x.CaseLetterId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
