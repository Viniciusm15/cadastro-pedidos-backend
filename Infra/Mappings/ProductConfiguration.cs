using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.Mappings
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(p => new { p.Name, p.IsActive })
                .IsUnique()
                .HasFilter("[IsActive] = 1");

            builder.Property(p => p.Description)
                .IsRequired();

            builder.Property(p => p.Price)
                .IsRequired();

            builder.Property(p => p.StockQuantity)
                .IsRequired();

            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);
        }
    }
}
