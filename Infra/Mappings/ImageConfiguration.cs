using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.Mappings
{
    public class ImageConfiguration : IEntityTypeConfiguration<Image>
    {
        public void Configure(EntityTypeBuilder<Image> builder)
        {
            builder.ToTable("Images");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.ImageData)
                .IsRequired()
                .HasColumnType("varbinary(max)");

            builder.Property(i => i.ImageMimeType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(i => i.Description)
                .HasMaxLength(255);

            builder.Property(i => i.EntityId)
                .IsRequired();

            builder.Property(i => i.EntityType)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(i => new { i.EntityId, i.EntityType })
                .IsUnique();
        }
    }
}
