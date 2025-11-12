# Testing Strategy

## 🎯 Comprehensive Testing Approach

This document outlines the complete testing strategy for the Payment Authorization System.

---

## Testing Philosophy

> **"Tests are not just about finding bugs—they're about designing better code and providing living documentation."**

### Core Principles
1. **Test First:** Write tests before implementation (TDD)
2. **Test Coverage:** Aim for >90% code coverage with meaningful tests
3. **Test Isolation:** Each test should be independent
4. **Test Speed:** Fast tests enable rapid feedback
5. **Test Clarity:** Tests should be easy to read and understand

---

## Test Pyramid

```
         /\
        /  \      ← E2E Tests (5-10%)
       /────\       • Full system integration
      /      \      • Slow but comprehensive
     /────────\     • Test critical user flows
    /          \
   /  Integr.   \  ← Integration Tests (20-30%)
  /    Tests     \   • Component interaction
 /────────────────\  • Database, API, services
/                  \ • Medium speed
/    Unit Tests     \ ← Unit Tests (60-75%)
/____________________\  • Individual functions/classes
                        • Fast and isolated
                        • High coverage
```

---

## Test Categories

### 1. Unit Tests

**Purpose:** Test individual units of code in isolation

**Characteristics:**
- ✅ Fast execution (milliseconds)
- ✅ No external dependencies
- ✅ Use mocking/stubbing
- ✅ High volume

**What to Test:**
- Individual methods and functions
- Business logic
- Validation rules
- Edge cases and boundary conditions

**Example Structure:**
```csharp
public class CardValidatorTests
{
    private readonly Mock<ICardRepository> _mockRepository;
    private readonly CardValidator _validator;

    public CardValidatorTests()
    {
        _mockRepository = new Mock<ICardRepository>();
        _validator = new CardValidator(_mockRepository.Object);
    }

    [Fact]
    public async Task ValidateCardExists_WhenCardNotFound_ReturnsFailure()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByCardNumberAsync("123"))
            .ReturnsAsync((Card?)null);

        // Act
        var result = await _validator.ValidateCardExistsAsync("123");

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailureReason.Should().Contain("not found");
    }
}
```

**Test Projects:**
- `Authorizer.Core.Tests`
- `Authorizer.Application.Tests`
- `Authorizer.Infrastructure.Tests`
- `Authorizer.Api.Tests`

---

### 2. Integration Tests

**Purpose:** Test interaction between components

**Characteristics:**
- ⏱️ Slower than unit tests (seconds)
- 🔌 Use real dependencies (test database)
- 📦 Test component interaction
- 🎯 Medium volume

**What to Test:**
- Database operations (CRUD)
- Repository implementations
- Service layer with real dependencies
- API endpoints with test server

**Example Structure:**
```csharp
public class CardRepositoryIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly AuthorizerDbContext _context;

    public CardRepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _context = fixture.CreateContext();
    }

    [Fact]
    public async Task GetByCardNumberAsync_ReturnsCard()
    {
        // Arrange
        var card = new Card
        {
            CardId = Guid.NewGuid(),
            CardNumber = "4111111111111111",
            Status = CardStatus.Active
        };
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var repository = new CardRepository(_context);

        // Act
        var result = await repository.GetByCardNumberAsync("4111111111111111");

        // Assert
        result.Should().NotBeNull();
        result!.CardNumber.Should().Be("4111111111111111");
    }
}
```

**Test Projects:**
- `Authorizer.IntegrationTests`

---

### 3. End-to-End (E2E) Tests

**Purpose:** Test complete user workflows

**Characteristics:**
- 🐢 Slowest tests (seconds to minutes)
- 🌐 Full system integration
- 📸 Test real scenarios
- 🎯 Low volume

**What to Test:**
- Complete authorization flows
- Error handling paths
- Critical business scenarios

**Example Structure:**
```csharp
public class AuthorizationE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthorizationE2ETests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CompleteAuthorizationFlow_Success()
    {
        // Arrange - Setup test data in database
        await SeedTestDataAsync();
        
        var request = new AuthorizationRequest
        {
            CardNumber = "4111111111111111",
            Amount = 100.00m,
            MerchantCategoryCode = "5411",
            TransactionType = TransactionType.CardPresent
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authorization", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<AuthorizationResult>();
        result!.IsApproved.Should().BeTrue();
        result.AuthorizationCode.Should().NotBeNullOrEmpty();
    }
}
```

---

## Test Patterns and Practices

### Arrange-Act-Assert (AAA)

Always structure tests with clear sections:

```csharp
[Fact]
public async Task TestMethod()
{
    // Arrange - Setup test data and dependencies
    var testData = CreateTestData();
    var mockService = CreateMockService();

    // Act - Execute the code under test
    var result = await _sut.MethodUnderTest(testData);

    // Assert - Verify the outcome
    result.Should().BeTrue();
}
```

### Test Data Builders

Use builder pattern for complex test objects:

```csharp
public class CardBuilder
{
    private string _cardNumber = "4111111111111111";
    private CardStatus _status = CardStatus.Active;
    private string _cardholderName = "JOHN DOE";

    public CardBuilder WithCardNumber(string cardNumber)
    {
        _cardNumber = cardNumber;
        return this;
    }

    public CardBuilder WithStatus(CardStatus status)
    {
        _status = status;
        return this;
    }

    public CardBuilder Inactive()
    {
        _status = CardStatus.Inactive;
        return this;
    }

    public Card Build()
    {
        return new Card
        {
            CardId = Guid.NewGuid(),
            CardNumber = _cardNumber,
            CardholderName = _cardholderName,
            Status = _status,
            CreatedAt = DateTime.UtcNow
        };
    }
}

// Usage
var card = new CardBuilder()
    .WithCardNumber("4222222222222222")
    .Inactive()
    .Build();
```

### Object Mother Pattern

Create pre-configured test objects:

```csharp
public static class TestData
{
    public static Card ActiveCard(string cardNumber = "4111111111111111")
    {
        return new Card
        {
            CardId = Guid.NewGuid(),
            CardNumber = cardNumber,
            CardholderName = "JOHN DOE",
            Status = CardStatus.Active,
            ExpiryDate = DateTime.UtcNow.AddYears(2),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Card InactiveCard(string cardNumber = "4111111111111111")
    {
        var card = ActiveCard(cardNumber);
        card.Status = CardStatus.Inactive;
        return card;
    }

    public static AuthorizationRequest ValidRequest()
    {
        return new AuthorizationRequest
        {
            CardNumber = "4111111111111111",
            Amount = 100.00m,
            MerchantCategoryCode = "5411",
            TransactionType = TransactionType.CardPresent,
            RequestTimestamp = DateTime.UtcNow
        };
    }
}
```

### Test Fixtures

Share setup across test classes:

```csharp
public class DatabaseFixture : IDisposable
{
    private readonly string _connectionString;

    public DatabaseFixture()
    {
        _connectionString = $"Host=localhost;Database=test_{Guid.NewGuid()};";
        // Setup database
    }

    public AuthorizerDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AuthorizerDbContext>()
            .UseNpgsql(_connectionString)
            .Options;
        return new AuthorizerDbContext(options);
    }

    public void Dispose()
    {
        // Cleanup database
    }
}

public class MyTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public MyTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
}
```

---

## Testing Each Layer

### Core Layer Tests

**What to Test:**
- Entity creation and property assignment
- Value object equality
- Enum values
- Domain logic (if any)

**Example:**
```csharp
public class AddressTests
{
    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        var address1 = new Address
        {
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "US"
        };

        var address2 = new Address
        {
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "US"
        };

        address1.Equals(address2).Should().BeTrue();
    }
}
```

### Application Layer Tests

**What to Test:**
- Validators with mocked repositories
- Authorization engine orchestration
- Business rule enforcement
- Service methods

**Example:**
```csharp
public class AuthorizationEngineTests
{
    [Fact]
    public async Task Authorize_FailsOnFirstCriticalValidation_StopsProcessing()
    {
        // Arrange
        var mockCardValidator = new Mock<ICardValidator>();
        mockCardValidator
            .Setup(v => v.ValidateCardExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(ValidationResult.Failure("Card not found"));

        var mockAccountValidator = new Mock<IAccountValidator>();
        var mockTransactionValidator = new Mock<ITransactionValidator>();

        var engine = new AuthorizationEngine(
            mockCardValidator.Object,
            mockAccountValidator.Object,
            mockTransactionValidator.Object);

        var request = new AuthorizationRequest
        {
            CardNumber = "9999999999999999",
            Amount = 100.00m
        };

        // Act
        var result = await engine.AuthorizeAsync(request);

        // Assert
        result.IsApproved.Should().BeFalse();
        result.DeclineReason.Should().Contain("Card not found");
        
        // Verify other validators were NOT called (short-circuit)
        mockAccountValidator.Verify(
            v => v.ValidateAccountStatusAsync(It.IsAny<Guid>()), 
            Times.Never);
    }
}
```

### Infrastructure Layer Tests

**What to Test:**
- DbContext configuration
- Repository implementations
- Database queries
- Migration scripts

**Example:**
```csharp
public class TransactionRepositoryTests
{
    [Fact]
    public async Task GetRecentTransactionsAsync_ReturnsOnlyTransactionsInTimeWindow()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AuthorizerDbContext(options);
        
        var cardNumber = "4111111111111111";
        var now = DateTime.UtcNow;
        
        context.Transactions.AddRange(
            new Transaction { CardNumber = cardNumber, Timestamp = now.AddHours(-1) },  // In window
            new Transaction { CardNumber = cardNumber, Timestamp = now.AddHours(-2) },  // Out of window
            new Transaction { CardNumber = cardNumber, Timestamp = now.AddMinutes(-30) } // In window
        );
        await context.SaveChangesAsync();

        var repository = new TransactionRepository(context);

        // Act
        var result = await repository.GetRecentTransactionsAsync(
            cardNumber, 
            since: now.AddHours(-1.5));

        // Assert
        result.Should().HaveCount(2);
        result.All(t => t.Timestamp >= now.AddHours(-1.5)).Should().BeTrue();
    }
}
```

### API Layer Tests

**What to Test:**
- Controller actions
- Request validation
- Response formatting
- Error handling
- HTTP status codes

**Example:**
```csharp
public class AuthorizationControllerTests
{
    [Fact]
    public async Task Authorize_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var mockEngine = new Mock<IAuthorizationEngine>();
        var controller = new AuthorizationController(mockEngine.Object);
        
        var invalidRequest = new AuthorizationRequest
        {
            CardNumber = "",  // Invalid
            Amount = -100.00m  // Invalid
        };

        // Simulate model validation
        controller.ModelState.AddModelError("CardNumber", "Required");
        controller.ModelState.AddModelError("Amount", "Must be positive");

        // Act
        var result = await controller.Authorize(invalidRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Authorize_WhenEngineThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var mockEngine = new Mock<IAuthorizationEngine>();
        mockEngine
            .Setup(e => e.AuthorizeAsync(It.IsAny<AuthorizationRequest>()))
            .ThrowsAsync(new Exception("Database error"));

        var controller = new AuthorizationController(mockEngine.Object);
        var request = TestData.ValidRequest();

        // Act
        var result = await controller.Authorize(request);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
```

---

## Test Coverage Strategy

### Coverage Goals

| Layer | Target Coverage | Priority |
|-------|----------------|----------|
| Core | 95%+ | Critical |
| Application | 90%+ | High |
| Infrastructure | 85%+ | Medium |
| API | 80%+ | Medium |

### What to Cover

**Must Cover:**
- ✅ All business logic
- ✅ All validation rules
- ✅ Error handling paths
- ✅ Edge cases and boundaries

**Can Skip:**
- ⏭️ Auto-generated code
- ⏭️ Simple DTOs with no logic
- ⏭️ Third-party library code

### Measuring Coverage

```bash
# Install coverage tool
dotnet tool install --global dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Generate report
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report
```

---

## Test Data Management

### In-Memory Database (Fast Tests)

```csharp
var options = new DbContextOptionsBuilder<AuthorizerDbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    .Options;

using var context = new AuthorizerDbContext(options);
```

**Pros:** Fast, isolated
**Cons:** Doesn't test real database behavior

### Testcontainers (Real PostgreSQL)

```csharp
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public PostgresFixture()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public string ConnectionString => _container.GetConnectionString();
}
```

**Pros:** Tests real database behavior
**Cons:** Slower startup

---

## Continuous Testing

### Local Development

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Authorizer.Application.Tests

# Run tests in watch mode (TDD)
dotnet watch test

# Run tests with filter
dotnet test --filter "Category=Unit"
```

### CI/CD Pipeline

```yaml
# Example GitHub Actions
name: Test

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:16
        env:
          POSTGRES_PASSWORD: postgres
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Test
        run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
```

---

## Testing Anti-Patterns to Avoid

### ❌ Don't: Test Implementation Details

```csharp
// BAD - Testing private method
[Fact]
public void TestPrivateMethod()
{
    var result = _validator.GetType()
        .GetMethod("PrivateMethod", BindingFlags.NonPublic | BindingFlags.Instance)
        .Invoke(_validator, null);
}
```

### ✅ Do: Test Public Interface

```csharp
// GOOD - Testing observable behavior
[Fact]
public async Task ValidateCard_ReturnsExpectedResult()
{
    var result = await _validator.ValidateCardAsync("4111111111111111");
    result.IsValid.Should().BeTrue();
}
```

### ❌ Don't: Write Dependent Tests

```csharp
// BAD - Order matters
[Fact]
public void Test1_CreateCard() { /* creates card */ }

[Fact]
public void Test2_UpdateCard() { /* assumes card from Test1 exists */ }
```

### ✅ Do: Write Independent Tests

```csharp
// GOOD - Each test is self-contained
[Fact]
public void CreateCard_Success()
{
    // Arrange - Create everything needed for THIS test
    var card = TestData.ActiveCard();
    // ...
}
```

---

## Testing Checklist

Before marking a feature complete:

- [ ] All unit tests pass
- [ ] Integration tests pass
- [ ] Code coverage meets target (>90%)
- [ ] Edge cases tested
- [ ] Error scenarios tested
- [ ] Tests are independent
- [ ] Tests are readable
- [ ] No flaky tests
- [ ] Test names are descriptive
- [ ] Refactoring completed

---

## Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions](https://fluentassertions.com/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Martin Fowler - Test Pyramid](https://martinfowler.com/articles/practical-test-pyramid.html)
- [Kent Beck - Test Driven Development](https://www.amazon.com/Test-Driven-Development-Kent-Beck/dp/0321146530)

---

**Remember:** Good tests are an investment. They pay dividends in confidence, maintainability, and development speed!
