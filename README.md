# BookWise

![CI](https://github.com/abdulsaboor190/Multi-tenant-Booking-System/actions/workflows/ci.yml/badge.svg)

A multi-tenant booking platform where many independent businesses (clinics, salons, gyms) each run their own staff, services and availability, with their data isolated from each other. Built as a portfolio project.

> Status: in development. Phase 1 (foundation) is complete.

## Tech stack
- ASP.NET Core 10 Web API (layered: Domain, Application, Infrastructure, Api)
- Entity Framework Core + PostgreSQL
- Serilog, FluentValidation, OpenAPI (Scalar)
- xUnit tests, GitHub Actions CI
- Docker Compose for local PostgreSQL and a local test mail server (Mailpit)

## Prerequisites
- .NET 10 SDK
- Docker Desktop
- Git
- EF Core CLI: `dotnet tool install --global dotnet-ef`

## Run locally

1. Clone the repository
   ```
   git clone https://github.com/abdulsaboor190/Multi-tenant-Booking-System.git
   cd Multi-tenant-Booking-System
   ```
2. Start PostgreSQL and Mailpit
   ```
   docker compose up -d
   ```
3. Restore the .NET packages
   ```
   dotnet restore
   ```
4. Create the database tables
   ```
   dotnet ef database update --project src/BookWise.Infrastructure --startup-project src/BookWise.Api
   ```
5. Start the API
   ```
   dotnet run --project src/BookWise.Api --launch-profile http
   ```
6. Check it works
   - Health check: http://localhost:5099/health (should say Healthy)
   - API docs: http://localhost:5099/scalar/v1
   - Test inbox (Mailpit): http://localhost:8025

## Run the tests
```
dotnet test
```

## Project structure
```
src/
  BookWise.Domain/           business rules, no dependencies
  BookWise.Application/      use cases, validators, interfaces
  BookWise.Infrastructure/   database (EF Core), external services
  BookWise.Api/              HTTP endpoints, middleware, configuration
tests/
  BookWise.UnitTests/
  BookWise.IntegrationTests/
docs/
  PLAN.md                    phased execution plan
  DELIVERABLES.md            deliverables and progress tracker
  DECISIONS.md               design decisions and trade-offs
```

## Local development notes
- The database password in `appsettings.Development.json` is for the throwaway local Docker database only. Never use real secrets in committed files.
- Stop the services with `docker compose down` (data is kept). Use `docker compose down -v` to also delete the database data.
