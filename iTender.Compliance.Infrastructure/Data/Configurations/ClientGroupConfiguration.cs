using iTender.Compliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iTender.Compliance.Infrastructure.Data.Configurations
{
    public class ClientGroupConfiguration : IEntityTypeConfiguration<ClientGroup>
    {
        public void Configure(EntityTypeBuilder<ClientGroup> builder)
        {
            builder.ToTable("ClientGroups");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.NormalizedEmployerName)
                   .HasMaxLength(500)
                   .IsRequired();

            builder.Property(x => x.EmployerName)
                   .HasMaxLength(500)
                   .IsRequired();

            builder.Property(x => x.ContactName).HasMaxLength(200);
            builder.Property(x => x.ContactEmail).HasMaxLength(200);
            builder.Property(x => x.CcEmails).HasMaxLength(2000);

            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            builder.HasOne(x => x.Agent)
                   .WithMany()
                   .HasForeignKey(x => x.AgentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ConsolidatedLetter)
                   .WithMany()
                   .HasForeignKey(x => x.ConsolidatedLetterId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.NormalizedEmployerName);
            builder.HasIndex(x => x.Status);
        }
    }
}