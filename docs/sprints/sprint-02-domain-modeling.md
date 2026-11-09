# Sprint 2 — Domain Modeling & Database Architecture

**Project:** StockFlow  
**Sprint:** 02  
**Status:** Completed  
**Technology:** .NET 10, ASP.NET Core, C#, Entity Framework Core 10, PostgreSQL 16, Docker

---

## 1. Sprint Overview

Sprint 2 focused on establishing StockFlow's domain model and relational database architecture.

The objective was to transform the initial application foundation into a structured inventory management backend with domain entities, transaction models, Entity Framework Core configurations, PostgreSQL connectivity, database migrations, and development seed data.

By the end of this sprint, the application had a working PostgreSQL database containing all required tables, relationships, indexes, and sample records.

## 2. Technical Goals

- Design core inventory management domain entities.
- Model purchase and sales transactions.
- Establish entity relationships and navigation properties.
- Configure Entity Framework Core using Fluent API.
- Integrate PostgreSQL using Npgsql.
- Generate and apply the initial EF Core migration.
- Implement idempotent development seed data.
- Verify database integrity and connectivity.

## 3. Architecture

StockFlow follows Clean Architecture principles.

```text
StockFlow
│
├── StockFlow.Domain
│   ├── Common
│   ├── Entities
│   └── Enums
│
├── StockFlow.Application
│
├── StockFlow.Infrastructure
│   └── Persistence
│       ├── Configurations
│       ├── Migrations
│       ├── DbInitializer.cs
│       └── StockFlowDbContext.cs
│
└── StockFlow.Api
    └── Program.cs
```

### Layer responsibilities

| Layer | Responsibility |
|---|---|
| Domain | Business entities, base classes, and enums |
| Application | Application-level business logic |
| Infrastructure | EF Core, PostgreSQL, persistence, and database initialization |
| API | HTTP endpoints, dependency injection, and application startup |

The Domain layer remains independent of infrastructure concerns.

## 4. Domain Modeling

### 4.1 Base Entity

Created a shared `BaseEntity` containing:

- `Id` — GUID primary key.
- `CreatedAtUtc` — UTC creation timestamp.
- `UpdatedAtUtc` — optional update timestamp.

### 4.2 Core Entities

| Entity | Purpose |
|---|---|
| Category | Organizes products |
| Product | Stores product details, SKU, pricing, and category |
| Warehouse | Represents a physical storage location |
| Inventory | Tracks stock quantities by product and warehouse |
| Supplier | Stores supplier and contact information |

### 4.3 Transaction Entities

| Entity | Purpose |
|---|---|
| PurchaseOrder | Records procurement transactions |
| PurchaseOrderItem | Represents products within purchase orders |
| SalesOrder | Records customer sales transactions |
| SalesOrderItem | Represents products within sales orders |
| StockMovement | Tracks stock changes and movement types |

### 4.4 Enumerations

**OrderStatus**

- Draft
- Pending
- Confirmed
- Processing
- Completed
- Cancelled

**StockMovementType**

- Purchase
- Sale
- Adjustment
- TransferIn
- TransferOut
- Return

## 5. Entity Framework Core Configuration

Created `StockFlowDbContext` with DbSet properties for all ten domain entities.

Implemented separate `IEntityTypeConfiguration<T>` classes for:

- CategoryConfiguration
- ProductConfiguration
- WarehouseConfiguration
- InventoryConfiguration
- SupplierConfiguration
- PurchaseOrderConfiguration
- PurchaseOrderItemConfiguration
- SalesOrderConfiguration
- SalesOrderItemConfiguration
- StockMovementConfiguration

### Database configuration highlights

- Explicit PostgreSQL table naming.
- Primary key configuration.
- Required and optional fields.
- String length restrictions.
- Decimal precision for prices and quantities.
- Enum-to-string conversions.
- Foreign key relationships.
- Unique and composite indexes.
- Restrictive delete behavior for important relationships.

### Important indexes

| Table | Index |
|---|---|
| categories | Unique Name |
| products | Unique SKU |
| warehouses | Unique Code |
| purchase_orders | Unique OrderNumber |
| sales_orders | Unique OrderNumber |
| inventories | Unique ProductId + WarehouseId |

## 6. PostgreSQL Integration

Integrated PostgreSQL 16 using Docker Compose.

**Container:** `stockflow-postgres`

**Database:** `stockflow_db`

**Database user:** `stockflow_user`

**Host port:** `15432`

**Container port:** `5432`

### EF Core provider

Installed and configured:

- Microsoft.EntityFrameworkCore 10.0.0
- Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0
- Microsoft.EntityFrameworkCore.Design 10.0.0

Registered `StockFlowDbContext` using `UseNpgsql()` and the application's configured connection string.

## 7. Database Migration

Generated the initial migration:

```bash
dotnet ef migrations add InitialCreate \
  --project src/StockFlow.Infrastructure \
  --startup-project src/StockFlow.Api \
  --output-dir Persistence/Migrations
```

Applied the migration:

```bash
dotnet ef database update \
  --project src/StockFlow.Infrastructure \
  --startup-project src/StockFlow.Api
```

### Generated files

```text
Persistence/
└── Migrations/
    ├── 20261109210547_InitialCreate.cs
    ├── 20261109210547_InitialCreate.Designer.cs
    └── StockFlowDbContextModelSnapshot.cs
```

Successfully created all ten entity tables and the EF Core migration history table.

## 8. Development Seed Data

Implemented `DbInitializer.InitializeAsync()` to populate the development database.

### Seeded records

| Entity | Records |
|---|---:|
| Categories | 3 |
| Products | 4 |
| Warehouses | 2 |
| Suppliers | 2 |
| Inventory | 5 |
| **Total** | **16** |

### Sample products

| SKU | Product | Unit Price |
|---|---|---:|
| ELEC-001 | MacBook Pro | 1999.00 |
| ACC-001 | Mechanical Keyboard | 129.00 |
| ACC-002 | Wireless Mouse | 79.00 |
| OFF-001 | Notebook | 5.00 |

The initializer checks existing records before inserting them, preventing duplicate sample data when the development API restarts.

Seeding is restricted to the Development environment.

## 9. API Health Checks

Implemented the following endpoints:

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/health` | Application health |
| GET | `/api/health/db` | PostgreSQL connectivity |

### Successful database health response

```json
{
  "status": "connected",
  "database": "stockflow_db",
  "provider": "PostgreSQL"
}
```

## 10. Database Verification

Verified the PostgreSQL schema using `psql`.

### Tables created

1. categories
2. products
3. warehouses
4. inventories
5. suppliers
6. purchase_orders
7. purchase_order_items
8. sales_orders
9. sales_order_items
10. stock_movements

Additional table: `__EFMigrationsHistory`.

### Integrity verification

| Verification | Result |
|---|---:|
| Entity tables | 10 |
| Unique indexes, including primary-key indexes | 17 |
| Foreign keys | 10 |
| Orphaned inventory records | 0 |
| Seeded categories | 3 |
| Seeded products | 4 |
| Seeded warehouses | 2 |
| Seeded suppliers | 2 |
| Seeded inventory records | 5 |

Verified the following relationships:

- Products → Categories
- Inventories → Products and Warehouses
- Purchase Orders → Suppliers
- Purchase Order Items → Purchase Orders and Products
- Sales Order Items → Sales Orders and Products
- Stock Movements → Products and Warehouses

### API verification

```bash
curl http://localhost:5080/api/health
curl http://localhost:5080/api/health/db
```

Both endpoints returned successful responses.

## 11. Challenges & Solutions

### PostgreSQL port conflict

The default PostgreSQL host port 5432 was already occupied by another project.

**Solution:** Configured StockFlow PostgreSQL to use host port 15432 while retaining container port 5432.

### API port conflict

An existing StockFlow API process was already listening on port 5080.

**Solution:** Identified the process using `lsof` and stopped the specific existing process before restarting the API.

### EF Core migration setup

Configured Infrastructure as the migration project and API as the startup project to ensure EF Core could resolve the registered DbContext and PostgreSQL provider.

### Idempotent seed data

Added existence checks before inserting sample categories, products, warehouses, suppliers, and inventory records.

## 12. Key Learnings

- Designing relational domain models with C# and EF Core.
- Applying Clean Architecture boundaries to persistence.
- Configuring entity relationships using Fluent API.
- Working with PostgreSQL and Docker Compose.
- Generating and applying EF Core migrations.
- Designing unique and composite database indexes.
- Implementing repeatable development database initialization.
- Validating relational integrity using PostgreSQL queries.

## 13. Sprint Completion Checklist

- [x] Core domain entities created.
- [x] Transaction entities created.
- [x] Domain enumerations implemented.
- [x] StockFlowDbContext created.
- [x] Entity configurations implemented.
- [x] PostgreSQL registered with EF Core.
- [x] Initial migration generated.
- [x] Database migration applied.
- [x] Ten entity tables verified.
- [x] Development seed data inserted.
- [x] Seed data counts verified.
- [x] Unique indexes verified.
- [x] Foreign keys verified.
- [x] Inventory integrity verified.
- [x] API health checks verified.
- [x] Database connectivity verified.

## 14. Next Sprint

**Sprint 3 — Product & Category Management**

Planned implementation:

- Category CRUD operations.
- Product CRUD operations.
- Product-category relationships.
- DTOs and validation.
- Application use cases and service abstractions.
- Repository and persistence integration.
- REST API endpoints.
- Pagination, filtering, and search.
- API testing and frontend integration.

---

**Sprint 2 Status: COMPLETED**

StockFlow now has a functioning domain model and PostgreSQL persistence foundation, ready for business feature development.
