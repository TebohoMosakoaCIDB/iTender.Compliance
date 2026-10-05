// iTender.Compliance.Infrastructure/Data/Configurations/DuplicateCandidateConfiguration.cs
using iTender.Compliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iTender.Compliance.Infrastructure.Data.Configurations
{
    public class DuplicateCandidateConfiguration : IEntityTypeConfiguration<DuplicateCandidate>
    {
        public void Configure(EntityTypeBuilder<DuplicateCandidate> builder)
        {
            builder.ToTable("DuplicateCandidates");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.MatchType)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(x => x.MatchedOn)
                   .HasMaxLength(500);

            builder.HasOne(x => x.Tender)
                   .WithMany()
                   .HasForeignKey(x => x.TenderId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.DuplicateGroupId);
            builder.HasIndex(x => x.TenderId);
        }
    }
}