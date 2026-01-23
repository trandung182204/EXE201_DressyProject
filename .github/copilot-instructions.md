# AI Coding Assistant Instructions for Dressy Project

## Project Overview
Dressy is a dress rental platform with ASP.NET Core backend API and placeholder frontend templates. Core entities: Providers (with branches), Products (with variants/images/reviews), Users, Bookings, Payments, Carts.

## Architecture
- **Backend**: ASP.NET Core 8 Web API with EF Core 8, SQL Server, JWT authentication, Swagger docs
- **Frontend**: Mirrored HTML templates (not active code); actual FE pending implementation
- **Data Flow**: Controllers → Services → Repositories → EF DbContext → SQL Server
- **Key Files**: `Program.cs` (DI setup), `ApplicationDbContext.cs` (EF models), Controllers/Services for business logic

## Key Patterns
- **Repository-Service Pattern**: All data access via `I{Entity}Repository` implementations, business logic in `I{Entity}Service`
- **API Responses**: Consistent format `{ success: bool, data: object, message: string }` (e.g., `ProductsController`)
- **Async/Await**: All I/O operations async (EF queries, service methods)
- **JWT Auth**: Bearer token in `Authorization` header; configure in `appsettings.json`
- **CORS**: Allows `localhost:5500/5501` for dev; update for production URLs

## Conventions
- **Table Naming**: Lowercase with underscores (e.g., `booking_items`) despite SQL Server
- **Models**: Partial classes with navigation properties (scaffold from DB)
- **DTOs**: Used for complex requests (e.g., `CreateProductDto`); direct model binding for simple CRUD
- **Dependency Injection**: Scoped services/repositories in `Program.cs`
- **Error Handling**: Return `NotFound` with `{ success: false }` for missing entities

## Workflows
- **Build**: `dotnet build` in `BE/BE/` directory
- **Run API**: `dotnet run` in `BE/BE/`; Swagger at `https://localhost:{port}/swagger`
- **Database**: Local SQL Server (`DESKTOP-4C8DHR8\SQL2022`); migrations via EF tools if needed
- **Debug**: Attach debugger to `dotnet run`; use Swagger UI for API testing
- **Add Entity**: Create model, add DbSet in `ApplicationDbContext`, scaffold repository/service/controller

## Examples
- **Fetch Products**: `GET /api/products` returns `{ success: true, data: [...], message: "Fetched successfully" }`
- **Create Booking**: `POST /api/bookings` with JWT token; service handles commission calculation
- **Update Product Status**: Use `UpdateStatusAsync` in `ProductsService` for status changes

Focus on backend API development; frontend is template-only currently.</content>
<parameter name="filePath">d:\EXE201_Xúng Xính\EXE201_DressyProject\.github\copilot-instructions.md