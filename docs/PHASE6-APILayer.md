# Phase 6: API Layer

## Overview
Implement the API layer with ASP.NET Core Web API. This phase exposes the authorization engine through REST endpoints.

---

## 🔴 TDD REMINDER
For EVERY step:
1. ✅ Write the test FIRST
2. ✅ Run it and watch it FAIL (RED)
3. ✅ Write MINIMUM code to pass (GREEN)
4. ✅ Run test and see it PASS
5. ✅ Refactor if needed (REFACTOR)
6. ✅ Repeat

---

## Step 6.1: Create Authorization Controller

### TDD Cycle
- [ ] **RED:** Write controller tests
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Create `AuthorizationController`
- [ ] **GREEN:** Implement POST endpoint
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Optimize controller logic

### Test Implementation
**File: `tests/Authorizer.Api.Tests/Controllers/AuthorizationControllerTests.cs`**
```csharp
public class AuthorizationControllerTests
{
    [Fact]
    public async Task Authorize_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new AuthorizationRequest
        {
            CardNumber = "4111111111111111",
            Amount = 100.00m,
            MerchantCategoryCode = "5411"
        };
        
        var mockEngine = new Mock<IAuthorizationEngine>();
        mockEngine.Setup(e => e.AuthorizeAsync(It.IsAny<AuthorizationRequest>()))
                  .ReturnsAsync(AuthorizationResult.Approved(100.00m));
        
        var controller = new AuthorizationController(mockEngine.Object);

        // Act
        var result = await controller.Authorize(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<AuthorizationResult>().Subject;
        response.IsApproved.Should().BeTrue();
    }
    
    [Fact]
    public async Task Authorize_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var mockEngine = new Mock<IAuthorizationEngine>();
        var controller = new AuthorizationController(mockEngine.Object);
        controller.ModelState.AddModelError("CardNumber", "Required");

        // Act
        var result = await controller.Authorize(new AuthorizationRequest());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
```

### Implementation Tasks
- [ ] Create `src/Authorizer.Api/Controllers/AuthorizationController.cs`
- [ ] Add `[ApiController]` and `[Route]` attributes
- [ ] Implement POST `/api/authorization` endpoint
- [ ] Inject `IAuthorizationEngine`
- [ ] Add model validation
- [ ] Return appropriate HTTP status codes

### Validation
- [ ] All controller tests pass ✅
- [ ] Endpoint returns correct responses
- [ ] Validation works properly

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 6.2: Add Request/Response DTOs

### TDD Cycle
- [ ] **RED:** Write DTO mapping tests
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Create DTOs if needed
- [ ] **GREEN:** Add validation attributes
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Optimize DTOs

### Implementation Tasks
- [ ] Add validation attributes to `AuthorizationRequest`
- [ ] Ensure proper model validation
- [ ] Add Swagger annotations
- [ ] Document all properties

### Validation
- [ ] Model validation works ✅
- [ ] Swagger documentation is clear

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 6.3: Configure Dependency Injection

### Tasks
- [ ] Register all services in DI container
- [ ] Configure database connection
- [ ] Configure options pattern
- [ ] Add health checks

### Implementation
**File: `src/Authorizer.Api/Program.cs`**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register database
builder.AddNpgsqlDbContext<AuthorizerDbContext>("authorizerdb");

// Register repositories
builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Register validators
builder.Services.AddScoped<ICardValidator, CardValidator>();
builder.Services.AddScoped<IAccountValidator, AccountValidator>();
builder.Services.AddScoped<ITransactionValidator, TransactionValidator>();

// Register services
builder.Services.AddScoped<ICvc2ValidationService, Cvc2ValidationService>();
builder.Services.AddScoped<IAuthorizationEngine, AuthorizationEngine>();

// Configure options
builder.Services.Configure<AuthorizationOptions>(
    builder.Configuration.GetSection("Authorization"));

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("authorizerdb"));

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
```

### Validation
- [ ] All services are registered
- [ ] DI works correctly
- [ ] Health checks work

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 6.4: Add Configuration Files

### Tasks
- [ ] Create `appsettings.json`
- [ ] Create `appsettings.Development.json`
- [ ] Add authorization options
- [ ] Configure logging

### Implementation
**File: `src/Authorizer.Api/appsettings.json`**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Authorization": {
    "AllowedMerchantCategories": ["5411", "5812", "5999"],
    "MaxTransactionsPerHour": 5,
    "RateLimitWindowMinutes": 60
  }
}
```

### Validation
- [ ] Configuration loads correctly
- [ ] Options pattern works
- [ ] Settings can be overridden

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Phase 6 Completion Checklist

Before moving to Phase 7, ensure:
- [ ] Authorization controller is implemented
- [ ] All controller tests pass ✅
- [ ] Request validation works
- [ ] Dependency injection is configured
- [ ] Configuration files are set up
- [ ] Health checks work
- [ ] Swagger documentation is complete
- [ ] API returns proper HTTP status codes
- [ ] Code follows TDD principles
- [ ] Code is clean and well-documented

### 🎯 FINAL CHECKPOINT: Request comprehensive review of Phase 6

---

## Notes
- Use proper HTTP status codes (200, 400, 500)
- Add comprehensive Swagger documentation
- Consider rate limiting at API level
- Add request/response logging
- Always verify RED before GREEN

