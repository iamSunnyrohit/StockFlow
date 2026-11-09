using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Entities;

namespace StockFlow.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.ContactPerson)
            .HasMaxLength(150);

        builder.Property(x => x.Email)
            .HasMaxLength(254);

        builder.Property(x => x.Phone)
            .HasMaxLength(32);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.City)
            .HasMaxLength(100);

        builder.HasMany(x => x.PurchaseOrders)
            .WithOne(x => x.Supplier)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
