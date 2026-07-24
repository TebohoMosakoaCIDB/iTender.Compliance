using iTender.Compliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iTender.Compliance.Infrastructure.Data.Configurations
{
    public class CaseLetterConfiguration : IEntityTypeConfiguration<CaseLetter>
    {
        public void Configure(EntityTypeBuilder<CaseLetter> builder)
        {
            builder.ToTable("CaseLetters");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FileName)
                .HasMaxLength(250);

            builder.Property(x => x.FilePath)
                .HasMaxLength(500);

            builder.Property(x => x.RecipientEmail)
                .HasMaxLength(250);

            builder.HasOne(x => x.SigningRequest)
            .WithOne(x => x.CaseLetter)
            .HasForeignKey<SigningRequest>(x => x.CaseLetterId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
