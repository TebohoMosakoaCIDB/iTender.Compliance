using iTender.Compliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iTender.Compliance.Infrastructure.Data.Configurations
{
    public class ClientResponseConfiguration
        : IEntityTypeConfiguration<ClientResponse>
    {
        public void Configure(EntityTypeBuilder<ClientResponse> builder)
        {
            builder.ToTable("ClientResponses");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ClientComments)
                .HasMaxLength(4000);

            builder.Property(x => x.ReviewComments)
                .HasMaxLength(4000);

            builder.Property(x => x.Status)
                .IsRequired();

            builder.HasOne(x => x.ComplianceCase)
                .WithMany(x => x.ClientResponses)
                .HasForeignKey(x => x.ComplianceCaseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.CaseLetter)
                .WithMany(x => x.ClientResponses)
                .HasForeignKey(x => x.CaseLetterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PreviousResponse)
                .WithMany(x => x.Resubmissions)
                .HasForeignKey(x => x.PreviousResponseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ComplianceCaseId);

            builder.HasIndex(x => x.CaseLetterId);

            builder.HasIndex(x => x.Status);

            builder.HasIndex(x => x.PreviousResponseId);
        }
    }
}