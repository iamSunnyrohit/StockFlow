using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Entities;

namespace StockFlow.Infrastructure.Persistence.Configurations;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("inventories");

        builder.HasKey(x => x.Id);
        
        builder.Property<uint>("xmin").IsRowVersion();
        builder.Property(x => x.QuantityOnHand)
            .HasPrecision(18, 3);

        builder.Property(x => x.ReorderLevel)
            .HasPrecision(18, 3);

        builder.HasIndex(x => new
        {
            x.ProductId,
            x.WarehouseId
        }).IsUnique();

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Inventories)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Warehouse)
            .WithMany(x => x.Inventories)
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
