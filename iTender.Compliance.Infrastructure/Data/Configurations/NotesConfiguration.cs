using iTender.Compliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iTender.Compliance.Infrastructure.Data.Configurations
{
    public class NotesConfiguration : IEntityTypeConfiguration<CaseNote>
    {
        public void Configure(EntityTypeBuilder<CaseNote> builder)
        {
            builder
                .HasOne(x => x.ComplianceCase)
                .WithMany(x => x.CaseNotes)
                .HasForeignKey(x => x.ComplianceCaseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
