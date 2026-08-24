using DocumentFormat.OpenXml.Vml.Office;
using iTender.Compliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iTender.Compliance.Infrastructure.Data.Configurations
{
    public class CorrespondenceTemplateConfiguration : IEntityTypeConfiguration<CorrespondenceTemplateModel>
    {        
        public void Configure(EntityTypeBuilder<CorrespondenceTemplateModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Subject)
                .HasMaxLength(500);

            builder.Property(x => x.Body)
                .IsRequired();

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.Version)
                .IsRequired();

            builder.Property(x => x.CreatedBy)
                .HasMaxLength(200);

            builder.Property(x => x.UpdatedBy)
                .HasMaxLength(200);

            builder.Property(x => x.ApprovedBy)
                .HasMaxLength(200);

            builder.Property(x => x.ApprovalComments)
                .HasMaxLength(2000);

            builder.HasIndex(x => new
            {
                x.Type,
                x.Version
            });

            builder.HasIndex(x => x.IsActive);
        }
    
    }
}
