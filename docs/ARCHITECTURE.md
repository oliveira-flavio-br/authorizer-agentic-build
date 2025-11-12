# Architecture Overview

## 🏛️ System Architecture

This document outlines the architecture and design decisions for the Payment Authorization System.

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    .NET Aspire Orchestrator                  │
│  (Service Discovery, Configuration, Health Checks, Telemetry)│
└─────────────────────────────────────────────────────────────┘
                            │
        ┌───────────────────┴───────────────────┐
        │                                       │
┌───────▼────────┐                    ┌────────▼────────┐
│  API Gateway   │                    │   PostgreSQL    │
│   (REST API)   │                    │    Database     │
└───────┬────────┘                    └─────────────────┘
        │
┌───────▼──────────────────────────────────────────────────────┐
│           Authorization Service                               │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  Authorization Engine (Orchestrator)                   │  │
│  └────────────────────────────────────────────────────────┘  │
│                         │                                     │
│     ┌──────────────────┼──────────────────┐                  │
│     │                  │                  │                  │
│  ┌──▼──────┐    ┌──────▼───────┐  ┌──────▼─────────┐       │
│  │  Card   │    │   Account    │  │  Transaction   │       │
│  │Controls │    │   Controls   │  │    Controls    │       │
│  └─────────┘    └──────────────┘  └────────────────┘       │
│                                                               │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  Data Access Layer (Entity Framework Core)            │  │
│  └────────────────────────────────────────────────────────┘  │
└───────────────────────────────────────────────────────────────┘
```

## Project Structure

```
authorizer-agentic-build/
├── src/
│   ├── Authorizer.AppHost/              # .NET Aspire orchestrator
│   │   └── Program.cs
│   │
│   ├── Authorizer.ServiceDefaults/      # Shared service configuration
│   │   └── Extensions.cs
│   │
│   ├── Authorizer.Api/                  # REST API entry point
│   │   ├── Controllers/
│   │   │   └── AuthorizationController.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── Authorizer.Core/                 # Domain models and interfaces
│   │   ├── Entities/
│   │   │   ├── Card.cs
│   │   │   ├── Account.cs
│   │   │   ├── Transaction.cs
│   │   │   └── AuthorizationRequest.cs
│   │   ├── Enums/
│   │   │   ├── CardStatus.cs
│   │   │   ├── AccountStatus.cs
│   │   │   └── TransactionType.cs
│   │   ├── Interfaces/
│   │   │   ├── IAuthorizationEngine.cs
│   │   │   ├── ICardValidator.cs
│   │   │   ├── IAccountValidator.cs
│   │   │   └── ITransactionValidator.cs
│   │   └── ValueObjects/
│   │       ├── AuthorizationResult.cs
│   │       └── ValidationResult.cs
│   │
│   ├── Authorizer.Application/          # Business logic
│   │   ├── Services/
│   │   │   └── AuthorizationEngine.cs
│   │   ├── Validators/
│   │   │   ├── CardValidator.cs
│   │   │   ├── AccountValidator.cs
│   │   │   └── TransactionValidator.cs
│   │   └── Configuration/
│   │       └── AuthorizationOptions.cs
│   │
│   └── Authorizer.Infrastructure/       # Data access and external concerns
│       ├── Data/
│       │   ├── AuthorizerDbContext.cs
│       │   └── Configurations/
│       │       ├── CardConfiguration.cs
│       │       ├── AccountConfiguration.cs
│       │       └── TransactionConfiguration.cs
│       └── Repositories/
│           ├── CardRepository.cs
│           ├── AccountRepository.cs
│           └── TransactionRepository.cs
│
├── tests/
│   ├── Authorizer.Core.Tests/          # Domain model tests
│   ├── Authorizer.Application.Tests/   # Business logic tests
│   ├── Authorizer.Infrastructure.Tests/ # Data access tests
│   ├── Authorizer.Api.Tests/           # API tests
│   └── Authorizer.IntegrationTests/    # End-to-end tests
│
├── docs/
│   ├── TDD-GUIDE.md
│   ├── ARCHITECTURE.md
│   ├── DOMAIN-MODEL.md
│   ├── IMPLEMENTATION-PLAN.md
│   └── TESTING-STRATEGY.md
│
└── PROJECT-TRACKER.md
```

## Design Principles

### 1. Clean Architecture / Onion Architecture
- **Core:** Contains domain entities and business rules (no dependencies)
- **Application:** Contains business logic and orchestration
- **Infrastructure:** Contains data access and external integrations
- **API:** Contains HTTP endpoints and request/response handling

### 2. Dependency Inversion
- Core defines interfaces, Infrastructure implements them
- Dependencies flow inward toward the core
- Makes the system testable and maintainable

### 3. Single Responsibility Principle
- Each validator handles one type of control
- Separation of concerns between layers
- Clear, focused classes

### 4. Chain of Responsibility Pattern
- Authorization flows through a chain of validators
- Each validator can approve, decline, or pass to next
- Easy to add/remove controls

## Key Design Decisions

### Authorization Flow

```
Request → Authorization Engine
              ↓
    ┌─────────┴─────────┐
    │                   │
    ▼                   ▼
Card Validators   Account Validators
    │                   │
    └─────────┬─────────┘
              ↓
    Transaction Validators
              ↓
     Authorization Result
```

### Validation Strategy

Each validation returns a `ValidationResult`:
```csharp
public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? FailureReason { get; set; }
    public ValidationSeverity Severity { get; set; }
}
```

**Short-circuit on failure:** If any critical validation fails, stop processing.

### Database Strategy

- **PostgreSQL** for persistence
- **Entity Framework Core** for ORM
- **Migrations** for schema versioning
- **Connection pooling** via Aspire

### Transaction Management

```csharp
public async Task<AuthorizationResult> AuthorizeAsync(AuthorizationRequest request)
{
    using var transaction = await _dbContext.Database.BeginTransactionAsync();
    try
    {
        // Validate and process
        var result = await ProcessAuthorizationAsync(request);
        
        // Record transaction
        await _transactionRepository.AddAsync(result.Transaction);
        
        await transaction.CommitAsync();
        return result;
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

## Technology Choices

### .NET Aspire
**Why:** 
- Built-in service orchestration
- Automatic health checks and telemetry
- Easy local development with PostgreSQL
- Dashboard for observability

### Entity Framework Core
**Why:**
- Type-safe data access
- Migration support
- LINQ queries
- Well-tested ORM

### xUnit
**Why:**
- Modern, extensible
- Parallel test execution
- Excellent community support
- Built-in theory/data-driven tests

### PostgreSQL
**Why:**
- Robust and reliable
- Excellent performance
- JSON support (if needed)
- Wide adoption

## Security Considerations

### Sensitive Data
- **Card numbers:** Store securely, consider tokenization
- **CVC2:** Never store in database (validate only)
- **PII:** Encrypt at rest

### Validation
- Input validation at API layer
- Business validation at domain layer
- SQL injection prevention via parameterized queries

## Performance Considerations

### Database Optimization
- Indexes on frequently queried fields (CardNumber, AccountId)
- Connection pooling
- Async/await for I/O operations

### Caching Strategy (Future)
- Cache card/account data for active sessions
- Invalidate cache on updates
- Consider Redis for distributed caching

### Rate Limiting
- Track transaction counts in-memory or Redis
- Sliding window algorithm
- Configurable thresholds

## Monitoring and Observability

### Metrics
- Authorization approval/decline rates
- Response times
- Database query performance
- Error rates

### Logging
- Structured logging with Serilog
- Log all authorization decisions
- Include correlation IDs
- Sensitive data masking

### Health Checks
- Database connectivity
- API responsiveness
- Memory usage

## Testing Strategy

### Test Pyramid
```
        ┌─────────┐
        │   E2E   │  (Few - Full integration)
        ├─────────┤
        │Integration│  (Some - Component interaction)
        ├─────────┤
        │   Unit   │  (Many - Fast, isolated)
        └─────────┘
```

- **70%** Unit tests (fast, isolated)
- **20%** Integration tests (component interaction)
- **10%** End-to-end tests (full flow)

## Configuration Management

### appsettings.json
```json
{
  "Authorization": {
    "RateLimit": {
      "MaxTransactions": 5,
      "TimeWindowMinutes": 60
    },
    "AllowedMerchantCategories": [
      "5411", "5812", "5999"
    ]
  }
}
```

### Aspire Configuration
- Service endpoints
- Connection strings
- Health check intervals

## Future Considerations

### Scalability
- Horizontal scaling of API instances
- Database read replicas
- Event-driven architecture (if needed)

### Extensibility
- Plugin architecture for new validators
- Rule engine for complex scenarios
- External fraud detection integration

### Compliance
- PCI-DSS compliance requirements
- Audit logging
- Data retention policies

---

**Note:** This architecture emphasizes testability, maintainability, and clear separation of concerns while keeping implementation straightforward for the TDD approach.
