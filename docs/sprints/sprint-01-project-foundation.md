# Sprint 01 — Project Foundation & Development Environment

**Project:** StockFlow  
**Sprint:** 01  
**Status:** In Progress  
**Technology:** .NET 10, ASP.NET Core, React, TypeScript, PostgreSQL, Docker

---

## 1. Sprint Overview

Sprint 1 establishes the technical foundation of StockFlow, a full-stack inventory and order management platform.

The primary objective is to create a scalable project structure using Clean Architecture, configure the development environment, establish PostgreSQL connectivity, and prepare the frontend-to-backend communication layer.

## 2. Technical Objectives

- Initialize the .NET 10 backend solution.
- Establish Clean Architecture project boundaries.
- Configure Entity Framework Core and PostgreSQL.
- Set up PostgreSQL using Docker Compose.
- Create an ASP.NET Core API.
- Implement an initial health-check endpoint.
- Initialize a React + TypeScript frontend using Vite.
- Configure frontend API communication using Axios.
- Initialize Git and connect the project to GitHub.

## 3. Technology Stack

| Technology | Purpose |
|---|---|
| C# | Backend programming |
| .NET 10 | Backend framework |
| ASP.NET Core | REST API |
| Entity Framework Core | ORM |
| PostgreSQL | Relational database |
| Npgsql | PostgreSQL provider |
| Docker Compose | Local infrastructure |
| React | Frontend framework |
| TypeScript | Frontend type safety |
| Vite | Frontend build tooling |
| Axios | HTTP communication |
| Git & GitHub | Version control |

## 4. Backend Architecture

The backend follows Clean Architecture principles.

```text
backend/
└── src/
    ├── StockFlow.Api/
    ├── StockFlow.Application/
    ├── StockFlow.Domain/
    └── StockFlow.Infrastructure/
```

### Project responsibilities

**StockFlow.Api**
- HTTP endpoints
- Middleware
- API configuration
- Dependency injection entry point

**StockFlow.Application**
- Business use cases
- Application services
- DTOs and application contracts

**StockFlow.Domain**
- Core business entities
- Domain rules
- Business abstractions

**StockFlow.Infrastructure**
- Database access
- Entity Framework Core
- External infrastructure integrations

### Dependency direction

```text
API
 ├── Application
 └── Infrastructure
       ├── Application
       └── Domain

Application
 └── Domain

Domain
 └── No external project dependencies
```

## 5. Database Infrastructure

PostgreSQL is configured through Docker Compose.

**Container:** stockflow-postgres  
**Image:** postgres:16-alpine  
**Database:** stockflow_db  
**Username:** stockflow_user  
**Host Port:** 15432  
**Container Port:** 5432

The host port is 15432 because another development project already occupies port 5432.

### Database verification

```sql
SELECT current_database();
SELECT current_user;
```

Expected results:

- Database: `stockflow_db`
- User: `stockflow_user`

## 6. API Foundation

The ASP.NET Core application is configured with:

- Development OpenAPI support
- CORS policy for the React application
- Health-check endpoint

### Health endpoint

`GET /api/health`

Example response:

```json
{
  "status": "healthy",
  "application": "StockFlow API",
  "version": "1.0.0"
}
```

### Development server

```bash
dotnet run --urls http://localhost:5080
```

## 7. Frontend Foundation

The frontend uses React, TypeScript and Vite.

Planned structure:

```text
frontend/
└── src/
    ├── services/
    │   └── api.ts
    ├── App.tsx
    └── index.css
```

Axios is configured as a centralized HTTP client.

Frontend API URL:

```env
VITE_API_BASE_URL=http://localhost:5080
```

The frontend will use the API health endpoint to verify backend connectivity.

## 8. Development Environment

| Service | Local Address |
|---|---|
| React frontend | http://localhost:5173 |
| ASP.NET Core API | http://localhost:5080 |
| PostgreSQL | localhost:15432 |

## 9. Challenges & Solutions

### PostgreSQL port conflict

**Problem:** Port 5432 was already occupied by the GateFlow PostgreSQL container.

**Solution:** Changed the StockFlow host port to 15432 while retaining container port 5432.

### EF Core transitive dependency vulnerability

**Problem:** A vulnerable transitive dependency, `System.Security.Cryptography.Pkcs` 9.0.0, was detected.

**Solution:** Added the patched 9.0.21 package explicitly and verified the dependency tree.

## 10. Sprint Completion Checklist

- [x] .NET 10 solution initialized.
- [x] Clean Architecture projects created.
- [x] Project references configured.
- [x] EF Core and Npgsql packages installed.
- [x] Vulnerable dependency resolved.
- [x] Docker Compose configured.
- [x] PostgreSQL container started successfully.
- [x] PostgreSQL connection verified.
- [x] ASP.NET Core API initialized.
- [x] API health endpoint verified.
- [ ] React frontend initialized and verified.
- [ ] Frontend-to-backend communication verified.
- [ ] Root GitHub repository updated with sprint documentation.

## 11. Sprint Outcome

The backend foundation, database infrastructure and API health endpoint have been successfully established.

Frontend setup and end-to-end connectivity remain to be verified.

## 12. Next Sprint

**Sprint 02 — Domain Modeling & Database Architecture**

The next sprint will introduce the core inventory domain, entity relationships, database context, EF Core migrations and initial seed data.
