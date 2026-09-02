using iTender.Compliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iTender.Compliance.Infrastructure.Data.Configurations
{
    public class PublicHolidayConfiguration : IEntityTypeConfiguration<PublicHoliday>
    {
        public void Configure(EntityTypeBuilder<PublicHoliday> builder)
        {
            builder.ToTable("PublicHolidays");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(100);

            builder.HasIndex(x => x.Date)
                .IsUnique();
        }
    }
}