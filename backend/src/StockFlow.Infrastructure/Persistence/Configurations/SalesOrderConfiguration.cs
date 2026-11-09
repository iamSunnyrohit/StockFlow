using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Entities;

namespace StockFlow.Infrastructure.Persistence.Configurations;

public class SalesOrderConfiguration
    : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("sales_orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.OrderNumber)
            .IsUnique();

        builder.Property(x => x.CustomerName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.CustomerEmail)
            .HasMaxLength(254);

        builder.Property(x => x.CustomerPhone)
            .HasMaxLength(32);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.Subtotal)
            .HasPrecision(18, 2);

        builder.Property(x => x.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.SalesOrder)
            .HasForeignKey(x => x.SalesOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
