using Microsoft.EntityFrameworkCore;
using StockFlow.Domain.Entities;

namespace StockFlow.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        StockFlowDbContext context)
    {
        var categoryNames = new[]
        {
            "Electronics",
            "Accessories",
            "Office Supplies"
        };

        foreach (var name in categoryNames)
        {
            if (!await context.Categories.AnyAsync(c => c.Name == name))
            {
                context.Categories.Add(new Category
                {
                    Name = name,
                    Description = $"{name} products"
                });
            }
        }

        await context.SaveChangesAsync();

        var categories = await context.Categories
            .ToDictionaryAsync(c => c.Name, c => c.Id);

        var products = new[]
        {
            new
            {
                SKU = "ELEC-001",
                Name = "MacBook Pro",
                Category = "Electronics",
                UnitPrice = 1999.00m,
                UnitCost = 1600.00m
            },
            new
            {
                SKU = "ACC-001",
                Name = "Mechanical Keyboard",
                Category = "Accessories",
                UnitPrice = 129.00m,
                UnitCost = 75.00m
            },
            new
            {
                SKU = "ACC-002",
                Name = "Wireless Mouse",
                Category = "Accessories",
                UnitPrice = 79.00m,
                UnitCost = 40.00m
            },
            new
            {
                SKU = "OFF-001",
                Name = "Notebook",
                Category = "Office Supplies",
                UnitPrice = 5.00m,
                UnitCost = 2.50m
            }
        };

        foreach (var item in products)
        {
            if (!await context.Products.AnyAsync(p => p.SKU == item.SKU))
            {
                context.Products.Add(new Product
                {
                    SKU = item.SKU,
                    Name = item.Name,
                    CategoryId = categories[item.Category],
                    UnitPrice = item.UnitPrice,
                    UnitCost = item.UnitCost,
                    IsActive = true
                });
            }
        }

        var warehouseData = new[]
        {
            new
            {
                Code = "WH-MAIN",
                Name = "Main Warehouse",
                City = "Hyderabad"
            },
            new
            {
                Code = "WH-SEC",
                Name = "Secondary Warehouse",
                City = "Bengaluru"
            }
        };

        foreach (var item in warehouseData)
        {
            if (!await context.Warehouses.AnyAsync(w => w.Code == item.Code))
            {
                context.Warehouses.Add(new Warehouse
                {
                    Code = item.Code,
                    Name = item.Name,
                    City = item.City,
                    IsActive = true
                });
            }
        }

        var supplierData = new[]
        {
            new
            {
                Name = "TechSource Solutions",
                ContactPerson = "Alex Morgan",
                Email = "alex@techsource.example",
                Phone = "+91-9000000001"
            },
            new
            {
                Name = "Office Essentials",
                ContactPerson = "Sam Wilson",
                Email = "sam@officeessentials.example",
                Phone = "+91-9000000002"
            }
        };

        foreach (var item in supplierData)
        {
            if (!await context.Suppliers.AnyAsync(s => s.Name == item.Name))
            {
                context.Suppliers.Add(new Supplier
                {
                    Name = item.Name,
                    ContactPerson = item.ContactPerson,
                    Email = item.Email,
                    Phone = item.Phone,
                    IsActive = true
                });
            }
        }

        await context.SaveChangesAsync();

        var productIds = await context.Products
            .ToDictionaryAsync(p => p.SKU, p => p.Id);

        var warehouseIds = await context.Warehouses
            .ToDictionaryAsync(w => w.Code, w => w.Id);

        var inventoryData = new[]
        {
            new
            {
                SKU = "ELEC-001",
                Warehouse = "WH-MAIN",
                Quantity = 25m,
                ReorderLevel = 5m
            },
            new
            {
                SKU = "ACC-001",
                Warehouse = "WH-MAIN",
                Quantity = 80m,
                ReorderLevel = 15m
            },
            new
            {
                SKU = "ACC-002",
                Warehouse = "WH-MAIN",
                Quantity = 120m,
                ReorderLevel = 20m
            },
            new
            {
                SKU = "OFF-001",
                Warehouse = "WH-MAIN",
                Quantity = 200m,
                ReorderLevel = 30m
            },
            new
            {
                SKU = "ELEC-001",
                Warehouse = "WH-SEC",
                Quantity = 10m,
                ReorderLevel = 3m
            }
        };

        foreach (var item in inventoryData)
        {
            var productId = productIds[item.SKU];
            var warehouseId = warehouseIds[item.Warehouse];

            var exists = await context.Inventories.AnyAsync(i =>
                i.ProductId == productId &&
                i.WarehouseId == warehouseId);

            if (!exists)
            {
                context.Inventories.Add(new Inventory
                {
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    QuantityOnHand = item.Quantity,
                    ReorderLevel = item.ReorderLevel
                });
            }
        }

        await context.SaveChangesAsync();
    }
}