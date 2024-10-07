using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.Mappings
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.ToTable("Clients");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(c => new { c.Email, c.IsActive })
                .IsUnique()
                .HasFilter("[IsActive] = 1");

            builder.Property(c => c.Telephone)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(c => new { c.Telephone, c.IsActive })
                .IsUnique()
                .HasFilter("[IsActive] = 1");

            builder.Property(c => c.BirthDate)
                .HasColumnType("date");
        }
    }
}