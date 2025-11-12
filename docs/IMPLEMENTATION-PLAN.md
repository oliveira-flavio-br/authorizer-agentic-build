# Implementation Plan

## 🎯 Step-by-Step TDD Implementation Guide

This document provides a detailed, step-by-step plan for implementing the Payment Authorization System using Test-Driven Development.

---

## Phase 0: Project Setup (Foundation)

### Step 0.1: Initialize .NET Aspire Solution

**Goal:** Create the basic project structure with .NET Aspire

**Tasks:**
```bash
# Create Aspire solution
dotnet new aspire-starter -n Authorizer -o .

# Or create individual projects
dotnet new aspire-apphost -n Authorizer.AppHost
dotnet new aspire-servicedefaults -n Authorizer.ServiceDefaults
```

**Validation:** Solution builds successfully

---

### Step 0.2: Create Project Structure

**Goal:** Set up all projects following clean architecture

**Tasks:**
1. Create Core library (domain models)
2. Create Application library (business logic)
3. Create Infrastructure library (data access)
4. Create API project
5. Create test projects

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

**Validation:** All projects compile

---

### Step 0.3: Configure Project References

**Goal:** Set up proper dependency flow

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

# Configure test project references (each tests its corresponding project)
dotnet add tests/Authorizer.Core.Tests reference src/Authorizer.Core
dotnet add tests/Authorizer.Application.Tests reference src/Authorizer.Application
dotnet add tests/Authorizer.Infrastructure.Tests reference src/Authorizer.Infrastructure
dotnet add tests/Authorizer.Api.Tests reference src/Authorizer.Api
```

**Validation:** Dependency graph is correct

---

### Step 0.4: Add NuGet Packages

**Goal:** Install required dependencies

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

**Validation:** All packages restore successfully

---

### Step 0.5: Setup PostgreSQL with Aspire

**Goal:** Configure database connection

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

**Validation:** Can run Aspire and see PostgreSQL container start

---

## Phase 1: Card Controls (TDD)

### Control 1.1: Card Must Exist

#### Step 1.1.1: Create Domain Entities (Core)

**Test First!** Create `Authorizer.Core.Tests/Entities/CardTests.cs`

```csharp
public class CardTests
{
    [Fact]
    public void Card_WhenCreated_ShouldHaveValidProperties()
    {
        // Arrange & Act
        var card = new Card
        {
            CardId = Guid.NewGuid(),
            CardNumber = "4111111111111111",
            CardholderName = "JOHN DOE",
            Status = CardStatus.Active
        };

        // Assert
        Assert.NotEqual(Guid.Empty, card.CardId);
        Assert.Equal("4111111111111111", card.CardNumber);
        Assert.Equal(CardStatus.Active, card.Status);
    }
}
```

**Implementation:** Create `src/Authorizer.Core/Entities/Card.cs` and related enums

**Validation:** Test passes ✅

---

#### Step 1.1.2: Create Card Repository Interface

**Test First!** Create `Authorizer.Application.Tests/Validators/CardValidatorTests.cs`

```csharp
public class CardValidatorTests
{
    [Fact]
    public async Task ValidateCardExists_WhenCardNotFound_ShouldReturnFailure()
    {
        // Arrange
        var mockRepo = new Mock<ICardRepository>();
        mockRepo.Setup(r => r.GetByCardNumberAsync("1234567890123456"))
                .ReturnsAsync((Card?)null);
        
        var validator = new CardValidator(mockRepo.Object);

        // Act
        var result = await validator.ValidateCardExistsAsync("1234567890123456");

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailureReason.Should().Contain("Card not found");
    }

    [Fact]
    public async Task ValidateCardExists_WhenCardFound_ShouldReturnSuccess()
    {
        // Arrange
        var card = new Card { CardNumber = "4111111111111111" };
        var mockRepo = new Mock<ICardRepository>();
        mockRepo.Setup(r => r.GetByCardNumberAsync("4111111111111111"))
                .ReturnsAsync(card);
        
        var validator = new CardValidator(mockRepo.Object);

        // Act
        var result = await validator.ValidateCardExistsAsync("4111111111111111");

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
```

**Implementation Steps:**
1. Create `ICardRepository` interface in Core
2. Create `ValidationResult` value object in Core
3. Create `ICardValidator` interface in Core
4. Implement `CardValidator` in Application

**Validation:** Both tests pass ✅

---

### Control 1.2: Card Must Be Active

#### Step 1.2.1: Add Card Status Validation

**Test First!** Add to `CardValidatorTests.cs`

```csharp
[Theory]
[InlineData(CardStatus.Active, true)]
[InlineData(CardStatus.Inactive, false)]
[InlineData(CardStatus.Blocked, false)]
[InlineData(CardStatus.Expired, false)]
public async Task ValidateCardStatus_ReturnsExpectedResult(
    CardStatus status, bool expectedValid)
{
    // Arrange
    var card = new Card 
    { 
        CardNumber = "4111111111111111",
        Status = status 
    };
    
    var mockRepo = new Mock<ICardRepository>();
    mockRepo.Setup(r => r.GetByCardNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(card);
    
    var validator = new CardValidator(mockRepo.Object);

    // Act
    var result = await validator.ValidateCardStatusAsync("4111111111111111");

    // Assert
    result.IsValid.Should().Be(expectedValid);
}
```

**Implementation:** Add `ValidateCardStatusAsync` method to `CardValidator`

**Validation:** All tests pass ✅

---

### Control 1.3: CVC2 Validation (Card Not Present)

#### Step 1.3.1: Add CVC2 Validation

**Test First!** Add to `CardValidatorTests.cs`

```csharp
[Theory]
[InlineData("123", "123", true)]
[InlineData("123", "456", false)]
[InlineData("", "123", false)]
[InlineData("123", "", false)]
public async Task ValidateCvc2_ReturnsExpectedResult(
    string cardCvc, string requestCvc, bool expectedValid)
{
    // Arrange
    var mockRepo = new Mock<ICardRepository>();
    var mockCvcService = new Mock<ICvc2ValidationService>();
    
    mockCvcService.Setup(s => s.ValidateAsync(
        It.IsAny<string>(), cardCvc, requestCvc))
        .ReturnsAsync(cardCvc == requestCvc && !string.IsNullOrEmpty(cardCvc));
    
    var validator = new CardValidator(mockRepo.Object, mockCvcService.Object);

    // Act
    var result = await validator.ValidateCvc2Async("4111111111111111", requestCvc);

    // Assert
    result.IsValid.Should().Be(expectedValid);
}
```

**Implementation:** 
1. Create `ICvc2ValidationService` (simulates external validation)
2. Implement in Application layer
3. Update `CardValidator` to use the service

**Validation:** All tests pass ✅

---

### Control 1.4: Cardholder Name Validation (Card Not Present)

#### Step 1.4.1: Add Name Validation

**Test First!** Add to `CardValidatorTests.cs`

```csharp
[Theory]
[InlineData("JOHN DOE", "JOHN DOE", true)]
[InlineData("JOHN DOE", "john doe", true)]  // Case insensitive
[InlineData("JOHN DOE", "Jane Smith", false)]
[InlineData("JOHN DOE", "", false)]
public async Task ValidateCardholderName_ReturnsExpectedResult(
    string cardName, string requestName, bool expectedValid)
{
    // Arrange
    var card = new Card 
    { 
        CardNumber = "4111111111111111",
        CardholderName = cardName
    };
    
    var mockRepo = new Mock<ICardRepository>();
    mockRepo.Setup(r => r.GetByCardNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(card);
    
    var validator = new CardValidator(mockRepo.Object);

    // Act
    var result = await validator.ValidateCardholderNameAsync(
        "4111111111111111", requestName);

    // Assert
    result.IsValid.Should().Be(expectedValid);
}
```

**Implementation:** Add `ValidateCardholderNameAsync` to `CardValidator`

**Validation:** All tests pass ✅

---

## Phase 2: Account Controls (TDD)

### Control 2.1: Account Must Be Active

#### Step 2.1.1: Create Account Validator

**Test First!** Create `AccountValidatorTests.cs`

```csharp
public class AccountValidatorTests
{
    [Theory]
    [InlineData(AccountStatus.Active, true)]
    [InlineData(AccountStatus.Suspended, false)]
    [InlineData(AccountStatus.Closed, false)]
    [InlineData(AccountStatus.PendingActivation, false)]
    public async Task ValidateAccountStatus_ReturnsExpectedResult(
        AccountStatus status, bool expectedValid)
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new Account 
        { 
            AccountId = accountId,
            Status = status 
        };
        
        var mockRepo = new Mock<IAccountRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(accountId))
                .ReturnsAsync(account);
        
        var validator = new AccountValidator(mockRepo.Object);

        // Act
        var result = await validator.ValidateAccountStatusAsync(accountId);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }
}
```

**Implementation:** 
1. Create `Account` entity
2. Create `IAccountRepository`
3. Create `AccountValidator`

**Validation:** Tests pass ✅

---

### Control 2.2: Billing Address Validation

#### Step 2.2.1: Add Billing Address Validation

**Test First!** Add to `AccountValidatorTests.cs`

```csharp
[Fact]
public async Task ValidateBillingAddress_WhenMatches_ShouldReturnSuccess()
{
    // Arrange
    var accountId = Guid.NewGuid();
    var billingAddress = new Address
    {
        Street = "123 Main St",
        City = "Springfield",
        State = "IL",
        PostalCode = "62701",
        Country = "US"
    };
    
    var account = new Account 
    { 
        AccountId = accountId,
        BillingAddress = billingAddress
    };
    
    var mockRepo = new Mock<IAccountRepository>();
    mockRepo.Setup(r => r.GetByIdAsync(accountId))
            .ReturnsAsync(account);
    
    var validator = new AccountValidator(mockRepo.Object);

    // Act
    var result = await validator.ValidateBillingAddressAsync(
        accountId, billingAddress);

    // Assert
    result.IsValid.Should().BeTrue();
}

[Fact]
public async Task ValidateBillingAddress_WhenDoesNotMatch_ShouldReturnFailure()
{
    // Arrange
    var accountId = Guid.NewGuid();
    var accountAddress = new Address
    {
        Street = "123 Main St",
        City = "Springfield",
        State = "IL",
        PostalCode = "62701",
        Country = "US"
    };
    
    var requestAddress = new Address
    {
        Street = "456 Oak Ave",
        City = "Chicago",
        State = "IL",
        PostalCode = "60601",
        Country = "US"
    };
    
    var account = new Account 
    { 
        AccountId = accountId,
        BillingAddress = accountAddress
    };
    
    var mockRepo = new Mock<IAccountRepository>();
    mockRepo.Setup(r => r.GetByIdAsync(accountId))
            .ReturnsAsync(account);
    
    var validator = new AccountValidator(mockRepo.Object);

    // Act
    var result = await validator.ValidateBillingAddressAsync(
        accountId, requestAddress);

    // Assert
    result.IsValid.Should().BeFalse();
}
```

**Implementation:** 
1. Create `Address` value object with proper `Equals` override
2. Implement `ValidateBillingAddressAsync` in `AccountValidator`

**Validation:** Tests pass ✅

---

## Phase 3: Transaction Controls (TDD)

### Control 3.1: Rate Limiting

#### Step 3.1.1: Implement Rate Limit Validation

**Test First!** Create `TransactionValidatorTests.cs`

```csharp
public class TransactionValidatorTests
{
    [Fact]
    public async Task ValidateRateLimit_WhenUnderLimit_ShouldReturnSuccess()
    {
        // Arrange
        var cardNumber = "4111111111111111";
        var existingTransactions = new List<Transaction>
        {
            new() { CardNumber = cardNumber, Timestamp = DateTime.UtcNow.AddMinutes(-30) },
            new() { CardNumber = cardNumber, Timestamp = DateTime.UtcNow.AddMinutes(-20) }
        };
        
        var mockRepo = new Mock<ITransactionRepository>();
        mockRepo.Setup(r => r.GetRecentTransactionsAsync(
            cardNumber, It.IsAny<DateTime>()))
            .ReturnsAsync(existingTransactions);
        
        var validator = new TransactionValidator(mockRepo.Object);

        // Act
        var result = await validator.ValidateRateLimitAsync(
            cardNumber, maxTransactions: 5, timeWindow: TimeSpan.FromHours(1));

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateRateLimit_WhenOverLimit_ShouldReturnFailure()
    {
        // Arrange
        var cardNumber = "4111111111111111";
        var existingTransactions = Enumerable.Range(0, 5)
            .Select(_ => new Transaction 
            { 
                CardNumber = cardNumber, 
                Timestamp = DateTime.UtcNow.AddMinutes(-30) 
            })
            .ToList();
        
        var mockRepo = new Mock<ITransactionRepository>();
        mockRepo.Setup(r => r.GetRecentTransactionsAsync(
            cardNumber, It.IsAny<DateTime>()))
            .ReturnsAsync(existingTransactions);
        
        var validator = new TransactionValidator(mockRepo.Object);

        // Act
        var result = await validator.ValidateRateLimitAsync(
            cardNumber, maxTransactions: 5, timeWindow: TimeSpan.FromHours(1));

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailureReason.Should().Contain("rate limit");
    }
}
```

**Implementation:** 
1. Create `Transaction` entity
2. Create `ITransactionRepository`
3. Implement `ValidateRateLimitAsync` in `TransactionValidator`

**Validation:** Tests pass ✅

---

### Control 3.2: Merchant Category Code Validation

#### Step 3.2.1: Add MCC Validation

**Test First!** Add to `TransactionValidatorTests.cs`

```csharp
[Theory]
[InlineData("5411", true)]  // Allowed
[InlineData("5812", true)]  // Allowed
[InlineData("9999", false)] // Not allowed
public async Task ValidateMerchantCategory_ReturnsExpectedResult(
    string mcc, bool expectedValid)
{
    // Arrange
    var allowedCategories = new List<string> { "5411", "5812", "5999" };
    var options = Options.Create(new AuthorizationOptions
    {
        AllowedMerchantCategories = allowedCategories
    });
    
    var validator = new TransactionValidator(
        Mock.Of<ITransactionRepository>(), options);

    // Act
    var result = await validator.ValidateMerchantCategoryAsync(mcc);

    // Assert
    result.IsValid.Should().Be(expectedValid);
}
```

**Implementation:** 
1. Create `AuthorizationOptions` configuration class
2. Implement `ValidateMerchantCategoryAsync`

**Validation:** Tests pass ✅

---

### Control 3.3: Available Balance Validation

#### Step 3.3.1: Add Balance Validation

**Test First!** Add to `TransactionValidatorTests.cs`

```csharp
[Theory]
[InlineData(1000.00, 500.00, true)]   // Sufficient balance
[InlineData(1000.00, 1000.00, true)]  // Exact balance
[InlineData(1000.00, 1000.01, false)] // Insufficient
[InlineData(500.00, 1000.00, false)]  // Insufficient
public async Task ValidateAvailableBalance_ReturnsExpectedResult(
    decimal accountBalance, decimal transactionAmount, bool expectedValid)
{
    // Arrange
    var accountId = Guid.NewGuid();
    var account = new Account 
    { 
        AccountId = accountId,
        Balance = accountBalance
    };
    
    var mockAccountRepo = new Mock<IAccountRepository>();
    mockAccountRepo.Setup(r => r.GetByIdAsync(accountId))
                   .ReturnsAsync(account);
    
    var validator = new TransactionValidator(
        Mock.Of<ITransactionRepository>(),
        mockAccountRepo.Object);

    // Act
    var result = await validator.ValidateAvailableBalanceAsync(
        accountId, transactionAmount);

    // Assert
    result.IsValid.Should().Be(expectedValid);
}
```

**Implementation:** Implement `ValidateAvailableBalanceAsync`

**Validation:** Tests pass ✅

---

## Phase 4: Authorization Engine (Orchestration)

### Step 4.1: Create Authorization Engine

**Test First!** Create `AuthorizationEngineTests.cs`

```csharp
public class AuthorizationEngineTests
{
    [Fact]
    public async Task Authorize_WithValidRequest_ShouldApprove()
    {
        // Arrange
        var request = new AuthorizationRequest
        {
            CardNumber = "4111111111111111",
            Amount = 100.00m,
            MerchantCategoryCode = "5411",
            TransactionType = TransactionType.CardPresent
        };
        
        var mockCardValidator = new Mock<ICardValidator>();
        mockCardValidator.Setup(v => v.ValidateCardExistsAsync(It.IsAny<string>()))
                        .ReturnsAsync(ValidationResult.Success());
        mockCardValidator.Setup(v => v.ValidateCardStatusAsync(It.IsAny<string>()))
                        .ReturnsAsync(ValidationResult.Success());
        
        var mockAccountValidator = new Mock<IAccountValidator>();
        mockAccountValidator.Setup(v => v.ValidateAccountStatusAsync(It.IsAny<Guid>()))
                           .ReturnsAsync(ValidationResult.Success());
        
        var mockTransactionValidator = new Mock<ITransactionValidator>();
        mockTransactionValidator.Setup(v => v.ValidateRateLimitAsync(
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(ValidationResult.Success());
        mockTransactionValidator.Setup(v => v.ValidateMerchantCategoryAsync(It.IsAny<string>()))
            .ReturnsAsync(ValidationResult.Success());
        mockTransactionValidator.Setup(v => v.ValidateAvailableBalanceAsync(
            It.IsAny<Guid>(), It.IsAny<decimal>()))
            .ReturnsAsync(ValidationResult.Success());
        
        var engine = new AuthorizationEngine(
            mockCardValidator.Object,
            mockAccountValidator.Object,
            mockTransactionValidator.Object);

        // Act
        var result = await engine.AuthorizeAsync(request);

        // Assert
        result.IsApproved.Should().BeTrue();
        result.AuthorizationCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Authorize_WhenCardNotFound_ShouldDecline()
    {
        // Arrange
        var request = new AuthorizationRequest
        {
            CardNumber = "9999999999999999",
            Amount = 100.00m
        };
        
        var mockCardValidator = new Mock<ICardValidator>();
        mockCardValidator.Setup(v => v.ValidateCardExistsAsync(It.IsAny<string>()))
                        .ReturnsAsync(ValidationResult.Failure("Card not found"));
        
        var engine = new AuthorizationEngine(
            mockCardValidator.Object,
            Mock.Of<IAccountValidator>(),
            Mock.Of<ITransactionValidator>());

        // Act
        var result = await engine.AuthorizeAsync(request);

        // Assert
        result.IsApproved.Should().BeFalse();
        result.DeclineReason.Should().Contain("Card not found");
    }
}
```

**Implementation:** Create `AuthorizationEngine` that orchestrates all validators

**Validation:** Tests pass ✅

---

## Phase 5: Infrastructure Layer (Data Access)

### Step 5.1: Create DbContext

**Test First!** Create `AuthorizerDbContextTests.cs`

```csharp
public class AuthorizerDbContextTests
{
    [Fact]
    public async Task CanAddAndRetrieveCard()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AuthorizerDbContext(options);
        
        var card = new Card
        {
            CardId = Guid.NewGuid(),
            CardNumber = "4111111111111111",
            CardholderName = "JOHN DOE",
            Status = CardStatus.Active
        };

        // Act
        context.Cards.Add(card);
        await context.SaveChangesAsync();

        // Assert
        var retrieved = await context.Cards.FirstAsync();
        retrieved.CardNumber.Should().Be("4111111111111111");
    }
}
```

**Implementation:**
1. Create `AuthorizerDbContext`
2. Configure entity mappings
3. Create migrations

**Validation:** Tests pass ✅

---

### Step 5.2: Implement Repositories

**Test First!** Create `CardRepositoryTests.cs`

```csharp
public class CardRepositoryTests
{
    [Fact]
    public async Task GetByCardNumberAsync_WhenExists_ReturnsCard()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AuthorizerDbContext(options);
        var card = new Card
        {
            CardId = Guid.NewGuid(),
            CardNumber = "4111111111111111",
            Status = CardStatus.Active
        };
        context.Cards.Add(card);
        await context.SaveChangesAsync();

        var repository = new CardRepository(context);

        // Act
        var result = await repository.GetByCardNumberAsync("4111111111111111");

        // Assert
        result.Should().NotBeNull();
        result!.CardNumber.Should().Be("4111111111111111");
    }
}
```

**Implementation:** Create concrete repository implementations

**Validation:** Tests pass ✅

---

## Phase 6: API Layer

### Step 6.1: Create Authorization Controller

**Test First!** Create `AuthorizationControllerTests.cs`

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
}
```

**Implementation:** Create API controller with endpoints

**Validation:** Tests pass ✅

---

## Phase 7: Integration Tests

### Step 7.1: End-to-End Authorization Tests

**Test First!** Create `AuthorizationIntegrationTests.cs`

```csharp
public class AuthorizationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthorizationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Authorize_EndToEnd_ShouldProcessRequest()
    {
        // Arrange
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
        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<AuthorizationResult>();
        result.Should().NotBeNull();
    }
}
```

**Implementation:** Wire up all components with DI

**Validation:** End-to-end tests pass ✅

---

## Phase 8: Refactoring and Polish

### Tasks:
1. Remove code duplication
2. Improve naming
3. Add XML documentation
4. Configure logging
5. Add health checks
6. Run code coverage analysis
7. Update PROJECT-TRACKER.md

---

## TDD Best Practices Reminder

For EACH feature:
1. ✅ Write the test FIRST
2. ✅ Run it and watch it FAIL
3. ✅ Write MINIMUM code to pass
4. ✅ Run test and see it PASS
5. ✅ Refactor if needed
6. ✅ Repeat

---

## Next Steps

After completing the above, you can:
- Add more sophisticated rate limiting (sliding window)
- Implement caching strategies
- Add fraud detection rules
- Implement 3D Secure validation
- Add audit logging
- Performance optimization

**Remember:** TDD is not about perfection on the first try—it's about iterative improvement!
