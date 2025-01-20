using Domain.Enums;
using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.Mappings
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderDate)
                .IsRequired();

            builder.Property(o => o.TotalValue)
                .IsRequired();

            builder.Property(o => o.Status)
               .HasConversion(
                   status => status.ToString(),
                   status => (OrderStatus)Enum.Parse(typeof(OrderStatus), status)
               )
               .IsRequired();

            builder.HasOne(o => o.Client)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.ClientId);
        }
    }
}
