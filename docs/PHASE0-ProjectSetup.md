# Phase 0: Project Setup (Foundation)

## Overview
This phase establishes the foundational project structure using .NET Aspire and sets up the development environment.

---

## Step 0.1: Initialize .NET Aspire Solution

### Goal
Create the basic project structure with .NET Aspire

### Tasks
- [ ] Create Aspire solution using starter template OR individual projects
- [ ] Verify solution structure is created correctly
- [ ] Build solution to ensure it compiles

### Commands
```bash
# Option 1: Create Aspire solution with starter template
dotnet new aspire-starter -n Authorizer -o .

# Option 2: Create individual projects
dotnet new aspire-apphost -n Authorizer.AppHost
dotnet new aspire-servicedefaults -n Authorizer.ServiceDefaults
```

### Validation
- [ ] Solution builds successfully without errors
- [ ] All Aspire infrastructure projects are present

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 0.2: Create Project Structure

### Goal
Set up all projects following clean architecture principles

### Tasks
- [ ] Create Core library (domain models)
- [ ] Create Application library (business logic)
- [ ] Create Infrastructure library (data access)
- [ ] Create API project
- [ ] Create test projects for each layer
- [ ] Add all projects to solution file

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

# Add all projects to solution
dotnet sln add src/**/*.csproj tests/**/*.csproj
```

### Validation
- [ ] All projects compile successfully
- [ ] Solution structure follows clean architecture
- [ ] Test projects are properly organized

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 0.3: Configure Project References

### Goal
Set up proper dependency flow between projects

### Tasks
- [ ] Application references Core
- [ ] Infrastructure references Core and Application
- [ ] API references all layers
- [ ] Each test project references its corresponding project
- [ ] Verify dependency graph is acyclic

### Commands
```bash
# Application references Core
dotnet add src/Authorizer.Application reference src/Authorizer.Core

# Infrastructure references Core and Application
dotnet add src/Authorizer.Infrastructure reference src/Authorizer.Core
dotnet add src/Authorizer.Infrastructure reference src/Authorizer.Application

# API references all
dotnet add src/Authorizer.Api reference src/Authorizer.Core
dotnet add src/Authorizer.Api reference src/Authorizer.Application
dotnet add src/Authorizer.Api reference src/Authorizer.Infrastructure

# Configure test project references
dotnet add tests/Authorizer.Core.Tests reference src/Authorizer.Core
dotnet add tests/Authorizer.Application.Tests reference src/Authorizer.Application
dotnet add tests/Authorizer.Infrastructure.Tests reference src/Authorizer.Infrastructure
dotnet add tests/Authorizer.Api.Tests reference src/Authorizer.Api
```

### Validation
- [ ] All project references are correctly configured
- [ ] No circular dependencies exist
- [ ] Solution builds successfully

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 0.4: Add NuGet Packages

### Goal
Install all required dependencies for the project

### Tasks
- [ ] Add EF Core and PostgreSQL packages to Infrastructure
- [ ] Add Aspire packages to API
- [ ] Add testing libraries to test projects
- [ ] Verify all packages restore successfully

### Commands
```bash
# Infrastructure - EF Core and PostgreSQL
dotnet add src/Authorizer.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/Authorizer.Infrastructure package Microsoft.EntityFrameworkCore.Design

# API - Aspire
dotnet add src/Authorizer.Api package Aspire.Npgsql.EntityFrameworkCore.PostgreSQL

# Test projects - Testing libraries
dotnet add tests/Authorizer.Application.Tests package Moq
dotnet add tests/Authorizer.Application.Tests package FluentAssertions
dotnet add tests/Authorizer.Infrastructure.Tests package Microsoft.EntityFrameworkCore.InMemory
dotnet add tests/Authorizer.IntegrationTests package Testcontainers.PostgreSql
```

### Validation
- [ ] All packages restore without errors
- [ ] No package version conflicts
- [ ] Solution builds successfully with all packages

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 0.5: Setup PostgreSQL with Aspire

### Goal
Configure database connection using .NET Aspire

### Tasks
- [ ] Configure PostgreSQL in AppHost
- [ ] Set up database reference
- [ ] Configure API to use PostgreSQL
- [ ] Test Aspire can start PostgreSQL container

### Implementation
**File: `src/Authorizer.AppHost/Program.cs`**
```csharp
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("authorizerdb");

var api = builder.AddProject<Projects.Authorizer_Api>("authorizer-api")
    .WithReference(postgres);

builder.Build().Run();
```

### Validation
- [ ] Aspire AppHost project runs without errors
- [ ] PostgreSQL container starts successfully
- [ ] Dashboard is accessible
- [ ] Database connection string is configured

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Phase 0 Completion Checklist

Before moving to Phase 1, ensure:
- [ ] All projects are created and organized
- [ ] Project references are correctly configured
- [ ] All NuGet packages are installed
- [ ] PostgreSQL is configured with Aspire
- [ ] Solution builds successfully
- [ ] All validation steps passed

### 🎯 FINAL CHECKPOINT: Request comprehensive review of Phase 0

---

## Notes
- Keep the structure clean and organized
- Follow .NET naming conventions
- Ensure no unnecessary dependencies
- Document any deviations from the plan

