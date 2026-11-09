using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StockFlow.Infrastructure.Persistence;

#nullable disable

namespace StockFlow.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(StockFlowDbContext))]
    partial class StockFlowDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("StockFlow.Domain.Entities.Category", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime?>("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("categories", (string)null);
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.Inventory", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("QuantityOnHand")
                        .HasPrecision(18, 3)
                        .HasColumnType("numeric(18,3)");

                    b.Property<decimal>("ReorderLevel")
                        .HasPrecision(18, 3)
                        .HasColumnType("numeric(18,3)");

                    b.Property<DateTime?>("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("WarehouseId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("WarehouseId");

                    b.HasIndex("ProductId", "WarehouseId")
                        .IsUnique();

                    b.ToTable("inventories", (string)null);
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.Product", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CategoryId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)");

                    b.Property<string>("SKU")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<decimal>("UnitCost")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<decimal>("UnitPrice")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<DateTime?>("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("SKU")
                        .IsUnique();

                    b.ToTable("products", (string)null);
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.PurchaseOrder", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("ExpectedDeliveryDateUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Notes")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<DateTime>("OrderDateUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("OrderNumber")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<decimal>("Subtotal")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<Guid>("SupplierId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("TaxAmount")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<decimal>("TotalAmount")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<DateTime?>("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("OrderNumber")
                        .IsUnique();

                    b.HasIndex("SupplierId");

                    b.ToTable("purchase_orders", (string)null);
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.PurchaseOrderItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("LineTotal")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("PurchaseOrderId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("Quantity")
                        .HasPrecision(18, 3)
                        .HasColumnType("numeric(18,3)");

                    b.Property<decimal>("UnitCost")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<DateTime?>("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ProductId");

                    b.HasIndex("PurchaseOrderId");

                    b.ToTable("purchase_order_items", (string)null);
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.SalesOrder", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CustomerEmail")
                        .HasMaxLength(254)
                        .HasColumnType("character varying(254)");

                    b.Property<string>("CustomerName")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)");

                    b.Property<string>("CustomerPhone")
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<string>("Notes")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<DateTime>("OrderDateUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("OrderNumber")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<decimal>("Subtotal")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<decimal>("TaxAmount")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<decimal>("TotalAmount")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<DateTime?>("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("OrderNumber")
                        .IsUnique();

                    b.ToTable("sales_orders", (string)null);
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.SalesOrderItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("LineTotal")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("Quantity")
                        .HasPrecision(18, 3)
                        .HasColumnType("numeric(18,3)");

                    b.Property<Guid>("SalesOrderId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("UnitPrice")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<DateTime?>("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ProductId");

                    b.HasIndex("SalesOrderId");

                    b.ToTable("sales_order_items", (string)null);
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.StockMovement", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Notes")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<DateTime>("OccurredAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("Quantity")
                        .HasPrecision(18, 3)
                        .HasColumnType("numeric(18,3)");

                    b.Property<string>("ReferenceNumber")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<DateTime?>("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("WarehouseId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("OccurredAtUtc");

                    b.HasIndex("ProductId");

                    b.HasIndex("WarehouseId");

                    b.ToTable("stock_movements", (string)null);
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.Supplier", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Address")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("City")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("ContactPerson")
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .HasMaxLength(254)
                        .HasColumnType("character varying(254)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)");

                    b.Property<string>("Phone")
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<DateTime?>("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("suppliers", (string)null);
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.Warehouse", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Address")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("City")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)");

                    b.Property<DateTime?>("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.ToTable("warehouses", (string)null);
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.Inventory", b =>
                {
                    b.HasOne("StockFlow.Domain.Entities.Product", "Product")
                        .WithMany("Inventories")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("StockFlow.Domain.Entities.Warehouse", "Warehouse")
                        .WithMany("Inventories")
                        .HasForeignKey("WarehouseId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Product");

                    b.Navigation("Warehouse");
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.Product", b =>
                {
                    b.HasOne("StockFlow.Domain.Entities.Category", "Category")
                        .WithMany("Products")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.PurchaseOrder", b =>
                {
                    b.HasOne("StockFlow.Domain.Entities.Supplier", "Supplier")
                        .WithMany("PurchaseOrders")
                        .HasForeignKey("SupplierId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Supplier");
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.PurchaseOrderItem", b =>
                {
                    b.HasOne("StockFlow.Domain.Entities.Product", "Product")
                        .WithMany("PurchaseOrderItems")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("StockFlow.Domain.Entities.PurchaseOrder", "PurchaseOrder")
                        .WithMany("Items")
                        .HasForeignKey("PurchaseOrderId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Product");

                    b.Navigation("PurchaseOrder");
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.SalesOrderItem", b =>
                {
                    b.HasOne("StockFlow.Domain.Entities.Product", "Product")
                        .WithMany("SalesOrderItems")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("StockFlow.Domain.Entities.SalesOrder", "SalesOrder")
                        .WithMany("Items")
                        .HasForeignKey("SalesOrderId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Product");

                    b.Navigation("SalesOrder");
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.StockMovement", b =>
                {
                    b.HasOne("StockFlow.Domain.Entities.Product", "Product")
                        .WithMany("StockMovements")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("StockFlow.Domain.Entities.Warehouse", "Warehouse")
                        .WithMany("StockMovements")
                        .HasForeignKey("WarehouseId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Product");

                    b.Navigation("Warehouse");
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.Category", b =>
                {
                    b.Navigation("Products");
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.Product", b =>
                {
                    b.Navigation("Inventories");

                    b.Navigation("PurchaseOrderItems");

                    b.Navigation("SalesOrderItems");

                    b.Navigation("StockMovements");
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.PurchaseOrder", b =>
                {
                    b.Navigation("Items");
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.SalesOrder", b =>
                {
                    b.Navigation("Items");
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.Supplier", b =>
                {
                    b.Navigation("PurchaseOrders");
                });

            modelBuilder.Entity("StockFlow.Domain.Entities.Warehouse", b =>
                {
                    b.Navigation("Inventories");

                    b.Navigation("StockMovements");
                });
#pragma warning restore 612, 618
        }
    }
}
