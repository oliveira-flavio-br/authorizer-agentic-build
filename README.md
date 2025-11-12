# Payment Authorization System

A comprehensive payment card authorization system built with .NET 8, implementing Test-Driven Development (TDD) principles.

## 🎯 Project Overview

This project implements a basic authorizer that applies various controls to authorization requests, similar to how payment processors like Mastercard handle card authorization flows. The focus is on implementing features using **Test-Driven Development** with a clean architecture approach.

## 🏗️ Architecture

The system is built using clean architecture principles with clear separation of concerns:

- **Core** - Domain entities, enums, and interfaces (no dependencies)
- **Application** - Business logic, validators, and orchestration
- **Infrastructure** - Data access, EF Core, PostgreSQL
- **API** - REST API endpoints using ASP.NET Core
- **AppHost** - .NET Aspire orchestration

## 🚀 Technology Stack

| Technology | Purpose |
|------------|---------|
| .NET 8 | Application framework |
| C# | Programming language |
| PostgreSQL | Database |
| Entity Framework Core | ORM and data access |
| xUnit | Testing framework |
| .NET Aspire | Service orchestration and observability |
| Moq | Mocking framework |
| FluentAssertions | Assertion library |

## 📋 Features (Requirements)

### Card Controls
- ✅ Approve only for cards we have issued
- ✅ Approve only for active cards
- ✅ Approve only for matching CVC2 for Card Not Present transactions
- ✅ Approve only for matching cardholder name for Card Not Present transactions

### Account Controls
- ✅ Approve only for active accounts
- ✅ Approve only for matching billing address for Card Not Present transactions

### Transaction Controls
- ✅ Rate limiting - x transactions within y time period
- ✅ Approved Merchant Category Codes (MCC)
- ✅ Transaction amount must be less than available balance

## 🗂️ Project Structure

```
authorizer-agentic-build/
├── src/
│   ├── Authorizer.AppHost/              # .NET Aspire orchestrator
│   ├── Authorizer.ServiceDefaults/      # Shared service defaults
│   ├── Authorizer.Api/                  # REST API
│   ├── Authorizer.Core/                 # Domain models
│   ├── Authorizer.Application/          # Business logic
│   └── Authorizer.Infrastructure/       # Data access
│
├── tests/
│   ├── Authorizer.Core.Tests/
│   ├── Authorizer.Application.Tests/
│   ├── Authorizer.Infrastructure.Tests/
│   ├── Authorizer.Api.Tests/
│   └── Authorizer.IntegrationTests/
│
├── docs/                                # Documentation
│   ├── TDD-GUIDE.md                    # TDD methodology guide
│   ├── ARCHITECTURE.md                 # Architecture decisions
│   ├── DOMAIN-MODEL.md                 # Domain entities
│   ├── IMPLEMENTATION-PLAN.md          # Step-by-step implementation
│   └── TESTING-STRATEGY.md             # Testing approach
│
├── PROJECT-TRACKER.md                   # Main progress tracker
└── README.md                            # This file
```

## 🚦 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for PostgreSQL via Aspire)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/authorizer-agentic-build.git
   cd authorizer-agentic-build
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run with .NET Aspire**
   ```bash
   cd src/Authorizer.AppHost
   dotnet run
   ```

4. **Access the application**
   - API: https://localhost:7xxx
   - Aspire Dashboard: https://localhost:15xxx

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Authorizer.Application.Tests

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Run tests in watch mode (TDD)
dotnet watch test --project tests/Authorizer.Application.Tests
```

## 📖 Documentation

Comprehensive documentation is available in the `/docs` folder:

- **[TDD Guide](docs/TDD-GUIDE.md)** - Learn the TDD approach used in this project
- **[TDD Cheatsheet](docs/TDD-CHEATSHEET.md)** - Quick reference for TDD commands and patterns ⚡
- **[Architecture](docs/ARCHITECTURE.md)** - System design and architectural decisions
- **[Domain Model](docs/DOMAIN-MODEL.md)** - Entities, value objects, and relationships
- **[Implementation Plan](docs/IMPLEMENTATION-PLAN.md)** - Step-by-step development guide
- **[Testing Strategy](docs/TESTING-STRATEGY.md)** - Testing patterns and practices
- **[Project Tracker](PROJECT-TRACKER.md)** - Track implementation progress

## 🧪 Test-Driven Development

This project strictly follows TDD principles:

1. **RED** - Write a failing test
2. **GREEN** - Write minimal code to pass
3. **REFACTOR** - Improve code quality

### TDD Workflow

```bash
# Terminal 1: Watch mode for tests (instant feedback)
dotnet watch test --project tests/Authorizer.Application.Tests

# Terminal 2: Development
# Write test → See it fail → Implement → See it pass → Refactor
```

### Test Coverage Goals

- Core: >95%
- Application: >90%
- Infrastructure: >85%
- API: >80%

## 🔌 API Usage

### Authorization Request

**POST** `/api/authorization`

```json
{
  "cardNumber": "4111111111111111",
  "cardholderName": "JOHN DOE",
  "cvc2": "123",
  "amount": 100.00,
  "merchantCategoryCode": "5411",
  "merchantName": "Test Merchant",
  "transactionType": 2,
  "billingAddress": {
    "street": "123 Main St",
    "city": "Springfield",
    "state": "IL",
    "postalCode": "62701",
    "country": "US"
  }
}
```

### Success Response (200 OK)

```json
{
  "isApproved": true,
  "authorizationCode": "123456",
  "amountAuthorized": 100.00,
  "processedAt": "2025-11-12T10:30:00Z",
  "validationMessages": []
}
```

### Decline Response (200 OK)

```json
{
  "isApproved": false,
  "declineReason": "Card is not active",
  "amountAuthorized": 0,
  "processedAt": "2025-11-12T10:30:00Z",
  "validationMessages": ["Card status is Inactive"]
}
```

## 🔧 Configuration

### appsettings.json

```json
{
  "Authorization": {
    "RateLimit": {
      "MaxTransactions": 5,
      "TimeWindowMinutes": 60
    },
    "AllowedMerchantCategories": [
      "5411",  // Grocery Stores
      "5812",  // Restaurants
      "5999",  // Miscellaneous Retail
      "5912",  // Drug Stores
      "5541"   // Service Stations
    ]
  }
}
```

## 🗄️ Database

The system uses PostgreSQL managed by .NET Aspire.

### Migrations

```bash
# Add migration
dotnet ef migrations add InitialCreate -p src/Authorizer.Infrastructure -s src/Authorizer.Api

# Update database
dotnet ef database update -p src/Authorizer.Infrastructure -s src/Authorizer.Api
```

## 📊 Monitoring

.NET Aspire provides built-in observability:

- **Health Checks** - Service health status
- **Telemetry** - Distributed tracing
- **Logs** - Structured logging
- **Metrics** - Performance metrics

Access the Aspire Dashboard at `https://localhost:15xxx` when running the AppHost.

## 🧩 Development Workflow

### Adding a New Feature

1. **Read the requirement** in [Implementation Plan](docs/IMPLEMENTATION-PLAN.md)
2. **Write a failing test** first (RED)
3. **Implement minimal code** to pass (GREEN)
4. **Refactor** while keeping tests green
5. **Update** [Project Tracker](PROJECT-TRACKER.md)
6. **Document** any architectural decisions

### Example TDD Flow

```csharp
// 1. Write failing test
[Fact]
public async Task ValidateCardStatus_InactiveCard_ReturnsFailure()
{
    // Arrange
    var card = new Card { Status = CardStatus.Inactive };
    _mockRepo.Setup(r => r.GetByCardNumberAsync("123")).ReturnsAsync(card);
    
    // Act
    var result = await _validator.ValidateCardStatusAsync("123");
    
    // Assert
    result.IsValid.Should().BeFalse();
}

// 2. Run test - it fails ❌
// 3. Implement ValidateCardStatusAsync
// 4. Run test - it passes ✅
// 5. Refactor if needed
```

## 🤝 Contributing

This is a learning project focused on TDD and agentic coding. Follow these guidelines:

1. Always write tests first (TDD)
2. Maintain >90% code coverage
3. Follow existing architecture patterns
4. Update documentation as you go
5. Use descriptive commit messages

## 📝 License

This project is for educational purposes.

## 🎓 Learning Resources

- [Test-Driven Development by Kent Beck](https://www.amazon.com/Test-Driven-Development-Kent-Beck/dp/0321146530)
- [Clean Architecture by Robert C. Martin](https://www.amazon.com/Clean-Architecture-Craftsmans-Software-Structure/dp/0134494164)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [xUnit Documentation](https://xunit.net/)

## 📈 Progress

Track the implementation progress in [PROJECT-TRACKER.md](PROJECT-TRACKER.md).

Current Status: **Setup & Planning**

---

**Built with ❤️ using Test-Driven Development**
