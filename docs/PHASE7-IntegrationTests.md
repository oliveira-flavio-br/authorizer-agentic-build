# Phase 7: Integration Tests

## Overview
Implement end-to-end integration tests using Testcontainers and WebApplicationFactory. This phase ensures all components work together correctly.

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

## Step 7.1: Setup Test Infrastructure

### Tasks
- [ ] Create test fixture with Testcontainers
- [ ] Configure WebApplicationFactory
- [ ] Setup test database
- [ ] Create test data helpers

### Implementation
**File: `tests/Authorizer.IntegrationTests/Infrastructure/AuthorizerApiFactory.cs`**
```csharp
public class AuthorizerApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;

    public AuthorizerApiFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithDatabase("authorizerdb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AuthorizerDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add test database
            services.AddDbContext<AuthorizerDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        // Apply migrations
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthorizerDbContext>();
        await context.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
```

### Validation
- [ ] Test factory works
- [ ] PostgreSQL container starts
- [ ] Migrations are applied

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 7.2: Create Test Data Helpers

### Implementation
**File: `tests/Authorizer.IntegrationTests/Helpers/TestDataBuilder.cs`**
```csharp
public class TestDataBuilder
{
    private readonly AuthorizerDbContext _context;

    public TestDataBuilder(AuthorizerDbContext context)
    {
        _context = context;
    }

    public async Task<Card> CreateTestCardAsync(
        string cardNumber = "4111111111111111",
        CardStatus status = CardStatus.Active)
    {
        var card = new Card
        {
            CardId = Guid.NewGuid(),
            CardNumber = cardNumber,
            CardholderName = "JOHN DOE",
            Status = status,
            AccountId = Guid.NewGuid()
        };
        
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();
        return card;
    }

    public async Task<Account> CreateTestAccountAsync(
        decimal balance = 1000.00m,
        AccountStatus status = AccountStatus.Active)
    {
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Status = status,
            Balance = balance,
            BillingAddress = new Address
            {
                Street = "123 Main St",
                City = "Springfield",
                State = "IL",
                PostalCode = "62701",
                Country = "US"
            }
        };
        
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }
}
```

### Validation
- [ ] Test data helpers work
- [ ] Can create test cards
- [ ] Can create test accounts

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 7.3: End-to-End Authorization Tests

### TDD Cycle
- [ ] **RED:** Write end-to-end test for successful authorization
- [ ] **RED:** Run test and verify it FAILS
- [ ] **GREEN:** Ensure all components are wired correctly
- [ ] **GREEN:** Run test and verify it PASSES
- [ ] **REFACTOR:** Clean up test code

### Test Implementation - Successful Authorization
**File: `tests/Authorizer.IntegrationTests/AuthorizationIntegrationTests.cs`**
```csharp
public class AuthorizationIntegrationTests : IClassFixture<AuthorizerApiFactory>
{
    private readonly HttpClient _client;
    private readonly AuthorizerApiFactory _factory;

    public AuthorizationIntegrationTests(AuthorizerApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Authorize_WithValidCard_ShouldApprove()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthorizerDbContext>();
        var testData = new TestDataBuilder(context);
        
        var account = await testData.CreateTestAccountAsync(balance: 1000.00m);
        var card = await testData.CreateTestCardAsync(
            cardNumber: "4111111111111111",
            status: CardStatus.Active);
        
        // Link card to account
        card.AccountId = account.AccountId;
        await context.SaveChangesAsync();
        
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
        result!.IsApproved.Should().BeTrue();
        result.AuthorizationCode.Should().NotBeNullOrEmpty();
    }
}
```

### Validation
- [ ] End-to-end test passes ✅
- [ ] All components work together
- [ ] Database operations work

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 7.4: Test Decline Scenarios

### TDD Cycle
- [ ] **RED:** Write tests for various decline scenarios
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Fix any issues in the pipeline
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Clean up test code

### Test Implementation - Various Declines
**Add to: `tests/Authorizer.IntegrationTests/AuthorizationIntegrationTests.cs`**
```csharp
[Fact]
public async Task Authorize_WithInsufficientFunds_ShouldDecline()
{
    // Arrange
    using var scope = _factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AuthorizerDbContext>();
    var testData = new TestDataBuilder(context);
    
    var account = await testData.CreateTestAccountAsync(balance: 50.00m);
    var card = await testData.CreateTestCardAsync();
    card.AccountId = account.AccountId;
    await context.SaveChangesAsync();
    
    var request = new AuthorizationRequest
    {
        CardNumber = "4111111111111111",
        Amount = 100.00m,
        MerchantCategoryCode = "5411"
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/authorization", request);

    // Assert
    response.Should().BeSuccessful();
    var result = await response.Content.ReadFromJsonAsync<AuthorizationResult>();
    result!.IsApproved.Should().BeFalse();
    result.DeclineReason.Should().Contain("funds");
}

[Fact]
public async Task Authorize_WithInactiveCard_ShouldDecline()
{
    // Arrange
    using var scope = _factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AuthorizerDbContext>();
    var testData = new TestDataBuilder(context);
    
    var account = await testData.CreateTestAccountAsync();
    var card = await testData.CreateTestCardAsync(status: CardStatus.Inactive);
    card.AccountId = account.AccountId;
    await context.SaveChangesAsync();
    
    var request = new AuthorizationRequest
    {
        CardNumber = "4111111111111111",
        Amount = 100.00m,
        MerchantCategoryCode = "5411"
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/authorization", request);

    // Assert
    response.Should().BeSuccessful();
    var result = await response.Content.ReadFromJsonAsync<AuthorizationResult>();
    result!.IsApproved.Should().BeFalse();
    result.DeclineReason.Should().Contain("inactive");
}
```

### Validation
- [ ] All decline scenario tests pass ✅
- [ ] Error messages are correct
- [ ] System handles failures gracefully

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 7.5: Test Rate Limiting

### TDD Cycle
- [ ] **RED:** Write rate limiting integration test
- [ ] **RED:** Run test and verify it FAILS
- [ ] **GREEN:** Ensure rate limiting works end-to-end
- [ ] **GREEN:** Run test and verify it PASSES
- [ ] **REFACTOR:** Optimize test

### Test Implementation
**Add to: `tests/Authorizer.IntegrationTests/AuthorizationIntegrationTests.cs`**
```csharp
[Fact]
public async Task Authorize_WhenRateLimitExceeded_ShouldDecline()
{
    // Arrange
    using var scope = _factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AuthorizerDbContext>();
    var testData = new TestDataBuilder(context);
    
    var account = await testData.CreateTestAccountAsync(balance: 10000.00m);
    var card = await testData.CreateTestCardAsync();
    card.AccountId = account.AccountId;
    await context.SaveChangesAsync();
    
    var request = new AuthorizationRequest
    {
        CardNumber = "4111111111111111",
        Amount = 100.00m,
        MerchantCategoryCode = "5411"
    };

    // Act - Make multiple requests to exceed rate limit
    var results = new List<AuthorizationResult>();
    for (int i = 0; i < 6; i++)
    {
        var response = await _client.PostAsJsonAsync("/api/authorization", request);
        var result = await response.Content.ReadFromJsonAsync<AuthorizationResult>();
        results.Add(result!);
    }

    // Assert
    results.Take(5).Should().OnlyContain(r => r.IsApproved);
    results.Last().IsApproved.Should().BeFalse();
    results.Last().DeclineReason.Should().Contain("rate limit");
}
```

### Validation
- [ ] Rate limiting test passes ✅
- [ ] Rate limiting works correctly
- [ ] Transaction history is maintained

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Phase 7 Completion Checklist

Before moving to Phase 8, ensure:
- [ ] Test infrastructure is set up
- [ ] Testcontainers work correctly
- [ ] Test data helpers are implemented
- [ ] Successful authorization test passes ✅
- [ ] Decline scenario tests pass ✅
- [ ] Rate limiting test passes ✅
- [ ] All integration tests pass ✅
- [ ] Tests are reliable and repeatable
- [ ] Test cleanup works properly
- [ ] Code follows TDD principles

### 🎯 FINAL CHECKPOINT: Request comprehensive review of Phase 7

---

## Notes
- Use Testcontainers for real database testing
- Clean up test data between tests
- Use unique card numbers for each test
- Consider test isolation
- Always verify RED before GREEN
- Integration tests should be reliable

