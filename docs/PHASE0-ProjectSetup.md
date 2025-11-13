# Phase 0: Project Setup (Foundation)

## Overview
This phase establishes the foundational project structure using .NET Aspire and sets up the development environment.

---

## Step 0.1: Initialize .NET Aspire Solution ✅

### Goal
Create the basic project structure with .NET Aspire

### Tasks
- [x] Create Aspire solution using starter template OR individual projects
- [x] Verify solution structure is created correctly
- [x] Build solution to ensure it compiles

### Commands
```bash
# Option 1: Create Aspire solution with starter template
dotnet new aspire-starter -n Authorizer -o .

# Option 2: Create individual projects (USED)
dotnet new aspire-apphost -n Authorizer.AppHost -o src/Authorizer.AppHost
dotnet new aspire-servicedefaults -n Authorizer.ServiceDefaults -o src/Authorizer.ServiceDefaults
dotnet new sln -n Authorizer
dotnet sln add src/Authorizer.AppHost src/Authorizer.ServiceDefaults
```

### Implementation Notes
- Used Option 2 (individual projects) for more control
- Created local `nuget.config` to use only nuget.org (resolved Azure DevOps feed auth issues)
- All projects targeting .NET 8

### Validation
- [x] Solution builds successfully without errors
- [x] All Aspire infrastructure projects are present

### ✅ CHECKPOINT: Completed - Commit `c440a4f` - Pushed to GitHub

---

## Step 0.2: Create Project Structure ✅

### Goal
Set up all projects following clean architecture principles

### Tasks
- [x] Create Core library (domain models)
- [x] Create Application library (business logic)
- [x] Create Infrastructure library (data access)
- [x] Create API project
- [x] Create test projects for each layer
- [x] Add all projects to solution file

### Commands
```bash
# Core library
dotnet new classlib -n Authorizer.Core -o src/Authorizer.Core

# Application library
dotnet new classlib -n Authorizer.Application -o src/Authorizer.Application

# Infrastructure library
dotnet new classlib -n Authorizer.Infrastructure -o src/Authorizer.Infrastructure

# API project
dotnet new webapi -n Authorizer.Api -o src/Authorizer.Api

# Test projects
dotnet new xunit -n Authorizer.Core.Tests -o tests/Authorizer.Core.Tests
dotnet new xunit -n Authorizer.Application.Tests -o tests/Authorizer.Application.Tests
dotnet new xunit -n Authorizer.Infrastructure.Tests -o tests/Authorizer.Infrastructure.Tests
dotnet new xunit -n Authorizer.Api.Tests -o tests/Authorizer.Api.Tests
dotnet new xunit -n Authorizer.IntegrationTests -o tests/Authorizer.IntegrationTests

# Add all projects to solution (PowerShell)
dotnet sln add (Get-ChildItem -Path src,tests -Filter *.csproj -Recurse | Select-Object -ExpandProperty FullName)
```

### Implementation Notes
- All projects created and added to solution (11 total projects)
- Updated all projects from .NET 9 to .NET 8 for consistency
- Removed OpenAPI dependencies from API project (not available in .NET 8.0)
- Clean architecture layers properly separated

### Validation
- [x] All projects compile successfully
- [x] Solution structure follows clean architecture
- [x] Test projects are properly organized

### ✅ CHECKPOINT: Completed - Commit `7bcb19f` - Pushed to GitHub

---

## Step 0.3: Configure Project References ✅

### Goal
Set up proper dependency flow between projects

### Tasks
- [x] Application references Core
- [x] Infrastructure references Core and Application
- [x] API references all layers
- [x] Each test project references its corresponding project
- [x] Verify dependency graph is acyclic

### Commands
```bash
# Application references Core
dotnet add src/Authorizer.Application reference src/Authorizer.Core

# Infrastructure references Core and Application
dotnet add src/Authorizer.Infrastructure reference src/Authorizer.Core
dotnet add src/Authorizer.Infrastructure reference src/Authorizer.Application

# API references all (including ServiceDefaults for Aspire)
dotnet add src/Authorizer.Api reference src/Authorizer.Core
dotnet add src/Authorizer.Api reference src/Authorizer.Application
dotnet add src/Authorizer.Api reference src/Authorizer.Infrastructure
dotnet add src/Authorizer.Api reference src/Authorizer.ServiceDefaults

# Configure test project references
dotnet add tests/Authorizer.Core.Tests reference src/Authorizer.Core
dotnet add tests/Authorizer.Application.Tests reference src/Authorizer.Application
dotnet add tests/Authorizer.Infrastructure.Tests reference src/Authorizer.Infrastructure
dotnet add tests/Authorizer.Api.Tests reference src/Authorizer.Api
dotnet add tests/Authorizer.IntegrationTests reference src/Authorizer.Api
```

### Implementation Notes
- Clean architecture dependency flow established
- API references ServiceDefaults for Aspire integration (health checks, telemetry, service discovery)
- IntegrationTests references API for full stack testing
- Dependency graph is acyclic and follows best practices

### Validation
- [x] All project references are correctly configured
- [x] No circular dependencies exist
- [x] Solution builds successfully

### ✅ CHECKPOINT: Completed - Commit `ad8e5d2` - Pushed to GitHub

---

## Step 0.4: Add NuGet Packages ✅

### Goal
Install all required dependencies for the project

### Tasks
- [x] Add EF Core and PostgreSQL packages to Infrastructure
- [x] Add Aspire packages to API
- [x] Add testing libraries to test projects
- [x] Verify all packages restore successfully

### Commands
```bash
# Infrastructure - EF Core and PostgreSQL (.NET 8 versions)
dotnet add src/Authorizer.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.10
dotnet add src/Authorizer.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 8.0.11

# API - Aspire
dotnet add src/Authorizer.Api package Aspire.Npgsql.EntityFrameworkCore.PostgreSQL

# Test projects - Testing libraries
dotnet add tests/Authorizer.Application.Tests package Moq
dotnet add tests/Authorizer.Application.Tests package FluentAssertions
dotnet add tests/Authorizer.Infrastructure.Tests package Microsoft.EntityFrameworkCore.InMemory --version 8.0.11
dotnet add tests/Authorizer.IntegrationTests package Testcontainers.PostgreSql
```

### Implementation Notes
**Infrastructure Packages:**
- Npgsql.EntityFrameworkCore.PostgreSQL 8.0.10 - PostgreSQL provider for EF Core
- Microsoft.EntityFrameworkCore.Design 8.0.11 - Design-time components for migrations

**API Packages:**
- Aspire.Npgsql.EntityFrameworkCore.PostgreSQL 9.5.2 - Aspire integration with health checks

**Test Packages:**
- Moq 4.20.72 - Mocking framework for unit tests
- FluentAssertions 8.8.0 - Fluent API for test assertions
- Microsoft.EntityFrameworkCore.InMemory 8.0.11 - In-memory database for testing
- Testcontainers.PostgreSql 4.8.1 - Docker PostgreSQL containers for integration tests

All packages selected for .NET 8 compatibility. Keeping it simple - additional packages (MediatR, FluentValidation, Swashbuckle) can be added later if needed.

### Validation
- [x] All packages restore without errors
- [x] No package version conflicts
- [x] Solution builds successfully with all packages

### ✅ CHECKPOINT: Completed - Commit `5f4d69b` - Pushed to GitHub

---

## Step 0.5: Setup PostgreSQL with Aspire ✅

### Goal
Configure database connection using .NET Aspire

### Tasks
- [x] Configure PostgreSQL in AppHost
- [x] Set up database reference
- [x] Configure API to use PostgreSQL
- [x] Test Aspire can start PostgreSQL container

### Implementation
**File: `src/Authorizer.AppHost/Program.cs`**
```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Configure PostgreSQL with Docker container and persistent volume
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("authorizerdb");

// Configure API with database reference
var api = builder.AddProject<Projects.Authorizer_Api>("authorizer-api")
    .WithReference(postgres);

builder.Build().Run();
```

**File: `src/Authorizer.Infrastructure/Data/AuthorizerDbContext.cs`**
```csharp
public class AuthorizerDbContext : DbContext
{
    public AuthorizerDbContext(DbContextOptions<AuthorizerDbContext> options) 
        : base(options) { }
    
    // Empty for now - DbSets will be added in Phase 1
}
```

**File: `src/Authorizer.Api/Program.cs`**
```csharp
using Authorizer.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (health checks, telemetry, service discovery)
builder.AddServiceDefaults();

// Add PostgreSQL database context with Aspire integration
builder.AddNpgsqlDbContext<AuthorizerDbContext>("authorizerdb");

var app = builder.Build();

// Map health check and telemetry endpoints
app.MapDefaultEndpoints();

// ... rest of configuration
```

### Implementation Notes
- **PostgreSQL runs in Docker container automatically** - Aspire handles Docker image pull and container lifecycle
- Added `Aspire.Hosting.PostgreSQL` package (v13.0.0) to AppHost
- Created minimal `AuthorizerDbContext` for connection testing (entities will be added in Phase 1)
- Configured with `.WithDataVolume()` for data persistence between container restarts
- Database name: `authorizerdb`
- ServiceDefaults provides health checks, telemetry, and service discovery
- AppHost dashboard available for monitoring (typically at `http://localhost:15000`)

### Validation
- [x] Aspire AppHost project runs without errors
- [x] PostgreSQL container starts successfully (Docker automatic)
- [x] Dashboard is accessible
- [x] Database connection string is configured
- [x] Solution builds successfully

### ✅ CHECKPOINT: Completed - Commit `f17c842` - Pushed to GitHub

---

## Phase 0 Completion Checklist

Before moving to Phase 1, ensure:
- [x] All projects are created and organized
- [x] Project references are correctly configured
- [x] All NuGet packages are installed
- [x] PostgreSQL is configured with Aspire
- [x] Solution builds successfully
- [x] All validation steps passed

### 🎯 PHASE 0 COMPLETE! ✅

## Implementation Progress

### Completed Steps:
- ✅ **Step 0.1**: Initialize .NET Aspire Solution (Commit: `c440a4f`)
- ✅ **Step 0.2**: Create Project Structure (Commit: `7bcb19f`)
- ✅ **Step 0.3**: Configure Project References (Commit: `ad8e5d2`)
- ✅ **Step 0.4**: Add NuGet Packages (Commit: `5f4d69b`)
- ✅ **Step 0.5**: Setup PostgreSQL with Aspire (Commit: `f17c842`)

### Status:
**🎉 PHASE 0 COMPLETE - Foundation Ready for Phase 1!**

---

## Notes
- Keep the structure clean and organized
- Follow .NET naming conventions
- Ensure no unnecessary dependencies
- Document any deviations from the plan

