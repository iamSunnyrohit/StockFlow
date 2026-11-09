# Sprint 3 — Product & Category Management

## Overview

Sprint 3 introduced the application layer and REST API functionality for managing product categories and products in StockFlow.

The sprint established DTO-based communication, repository abstractions, service-layer business logic, API controllers, centralized exception handling, product filtering, pagination, and automated API tests.

## Technical Goals

- Implement repository and service abstractions.
- Build category CRUD operations.
- Build product CRUD operations.
- Support product search, filtering, and pagination.
- Implement centralized API exception handling.
- Return consistent HTTP responses and ProblemDetails.
- Add automated API integration tests.

## Architecture

StockFlow follows Clean Architecture:

- **Domain:** Core entities and business models.
- **Application:** DTOs, interfaces, services, and application exceptions.
- **Infrastructure:** Entity Framework Core repositories.
- **API:** REST controllers, dependency injection, and exception middleware.
- **Tests:** Automated API tests using xUnit, WebApplicationFactory, and Moq.

## Implementation

### 3.1 Application Architecture

Implemented:

- Category and product DTOs.
- Create and update request models.
- Product query parameters.
- Generic paginated response model.
- Repository interfaces.
- Service interfaces.
- Category and product service implementations.
- Dependency injection registrations.

### 3.2 Category Management

Implemented endpoints:

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/categories` | Retrieve all categories |
| GET | `/api/categories/{id}` | Retrieve a category |
| POST | `/api/categories` | Create a category |
| PUT | `/api/categories/{id}` | Update a category |
| PATCH | `/api/categories/{id}/deactivate` | Deactivate a category |

Category deactivation uses soft deactivation through the `IsActive` property.

### 3.3 Product Management

Implemented endpoints:

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/products` | Retrieve products |
| GET | `/api/products/{id}` | Retrieve a product |
| POST | `/api/products` | Create a product |
| PUT | `/api/products/{id}` | Update a product |
| PATCH | `/api/products/{id}/deactivate` | Deactivate a product |

Product functionality includes:

- SKU uniqueness checks.
- Category validation.
- Product activation and deactivation.
- Product and category information in response DTOs.
- Monetary validation.

### 3.4 Validation and Error Handling

Implemented centralized exception handling with ProblemDetails responses.

| Exception | HTTP status |
|---|---:|
| NotFoundException | 404 |
| ConflictException | 409 |
| ValidationException | 400 |
| Unexpected exception | 500 |

The middleware also includes request trace identifiers and handles client-request cancellation.

### 3.5 Search, Filtering and Pagination

Product listing supports:

- Search by product name or SKU.
- Filtering by category ID.
- Filtering by active status.
- Page number and page size.
- Total count and total pages.
- Previous-page and next-page metadata.

Example:

`GET /api/products?page=1&pageSize=20&search=MacBook&isActive=true`

Maximum page size is restricted to 100.

## Database Changes

Sprint 3 uses the existing PostgreSQL schema established in Sprint 2.

No new database entities were required for this sprint.

Product and category operations use the existing EF Core configurations, relationships, and unique constraints.

## Automated Testing

Testing uses:

- xUnit
- Microsoft.AspNetCore.Mvc.Testing
- Moq
- WebApplicationFactory

Verified test scenarios include:

- Category listing.
- Missing category response.
- Successful category creation.
- Missing product response.
- Duplicate product SKU conflict.
- Invalid product data.
- Successful product creation.
- Paginated product listing.
- Pagination query parameter forwarding.

### Test Results

| Metric | Result |
|---|---:|
| Total tests | 9 |
| Passed | 9 |
| Failed | 0 |
| Skipped | 0 |

## Manual API Verification

Manually verified:

- Category creation, updating, and deactivation.
- Product creation, updating, and deactivation.
- Product search and category filtering.
- Active and inactive product filtering.
- Pagination.
- Duplicate SKU rejection.
- Negative price rejection.
- Missing resource responses.
- Inactive category validation.

## Challenges and Solutions

### Centralized Error Handling

Implemented custom application exceptions and middleware to produce consistent ProblemDetails responses.

### PostgreSQL Search

Used PostgreSQL's case-insensitive `ILIKE` functionality through EF Core.

### Pagination

Introduced a reusable `PagedResult<T>` response model to standardize paginated API responses.

### Automated API Testing

Used `WebApplicationFactory` and mocked application services so API tests do not depend on a running PostgreSQL database.

## Key Learnings

- Applying repository and service patterns in Clean Architecture.
- Separating API request models from domain entities.
- Implementing RESTful CRUD operations.
- Building reusable pagination.
- Centralizing API exception handling.
- Writing integration-style API tests with mocked dependencies.

## Sprint Completion Checklist

- [x] Application DTOs and interfaces
- [x] Category repository and service
- [x] Product repository and service
- [x] Category API endpoints
- [x] Product API endpoints
- [x] Validation and exception middleware
- [x] Search, filtering, and pagination
- [x] Automated API tests
- [x] Nine automated tests passing
- [x] Solution build successful
- [ ] Final package vulnerability review
- [ ] Git changes reviewed and committed

## Next Sprint

**Sprint 4 — Warehouse & Inventory Management**

Planned functionality:

- Warehouse management.
- Inventory repository and service.
- Stock quantity management.
- Inventory lookup by product and warehouse.
- Low-stock detection.
- Stock adjustments.
- Stock movement tracking.
- Inventory API endpoints and tests.
