using iTender.Compliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iTender.Compliance.Infrastructure.Data.Configurations
{
    public class AgentConfiguration
    : IEntityTypeConfiguration<Agent>
    {
        public void Configure(EntityTypeBuilder<Agent> builder)
        {
            builder.ToTable("Agents");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EmployeeNumber)
                .HasMaxLength(100);

            builder.HasIndex(x => x.UserId)
                .IsUnique();

            builder.Property(x => x.Level)
                .HasConversion<int>()
                .IsRequired();
        }
    }
}
