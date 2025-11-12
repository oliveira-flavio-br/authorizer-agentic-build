# Phase 5: Infrastructure Layer (Data Access)

## Overview
Implement the infrastructure layer with Entity Framework Core and PostgreSQL. This phase provides concrete implementations of repositories and data persistence.

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

## Step 5.1: Create DbContext

### TDD Cycle
- [ ] **RED:** Write DbContext tests
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Create `AuthorizerDbContext` class
- [ ] **GREEN:** Configure DbSets
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Optimize entity configurations

### Test Implementation - Basic CRUD
**File: `tests/Authorizer.Infrastructure.Tests/Data/AuthorizerDbContextTests.cs`**
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
    
    [Fact]
    public async Task CanAddAndRetrieveAccount()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AuthorizerDbContext(options);
        
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Status = AccountStatus.Active,
            Balance = 1000.00m
        };

        // Act
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        // Assert
        var retrieved = await context.Accounts.FirstAsync();
        retrieved.Balance.Should().Be(1000.00m);
    }
    
    [Fact]
    public async Task CanAddAndRetrieveTransaction()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AuthorizerDbContext(options);
        
        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            CardNumber = "4111111111111111",
            Amount = 100.00m,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Approved
        };

        // Act
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        // Assert
        var retrieved = await context.Transactions.FirstAsync();
        retrieved.Amount.Should().Be(100.00m);
    }
}
```

### Implementation Tasks
- [ ] Create `src/Authorizer.Infrastructure/Data/AuthorizerDbContext.cs`
- [ ] Add DbSet properties for Card, Account, Transaction
- [ ] Configure connection string handling
- [ ] Override `OnModelCreating` for entity configurations

### Validation
- [ ] All DbContext tests pass ✅
- [ ] Entities can be saved and retrieved
- [ ] In-memory database works correctly

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 5.2: Configure Entity Mappings

### TDD Cycle
- [ ] **RED:** Write entity configuration tests
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Create entity type configurations
- [ ] **GREEN:** Configure properties, indexes, relationships
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Optimize configurations

### Test Implementation
**Add to: `tests/Authorizer.Infrastructure.Tests/Data/AuthorizerDbContextTests.cs`**
```csharp
[Fact]
public void CardConfiguration_ShouldHaveUniqueIndex()
{
    // Arrange
    var options = new DbContextOptionsBuilder<AuthorizerDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    using var context = new AuthorizerDbContext(options);

    // Act
    var entityType = context.Model.FindEntityType(typeof(Card));
    var index = entityType.GetIndexes()
        .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(Card.CardNumber)));

    // Assert
    index.Should().NotBeNull();
    index.IsUnique.Should().BeTrue();
}

[Fact]
public async Task Account_ShouldStoreAddressAsOwnedType()
{
    // Arrange
    var options = new DbContextOptionsBuilder<AuthorizerDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    using var context = new AuthorizerDbContext(options);
    
    var account = new Account
    {
        AccountId = Guid.NewGuid(),
        Status = AccountStatus.Active,
        BillingAddress = new Address
        {
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "US"
        }
    };

    // Act
    context.Accounts.Add(account);
    await context.SaveChangesAsync();

    // Assert
    var retrieved = await context.Accounts
        .FirstAsync(a => a.AccountId == account.AccountId);
    retrieved.BillingAddress.Should().NotBeNull();
    retrieved.BillingAddress.Street.Should().Be("123 Main St");
}
```

### Implementation Tasks
- [ ] Create `src/Authorizer.Infrastructure/Data/Configurations/CardConfiguration.cs`
- [ ] Create `src/Authorizer.Infrastructure/Data/Configurations/AccountConfiguration.cs`
- [ ] Create `src/Authorizer.Infrastructure/Data/Configurations/TransactionConfiguration.cs`
- [ ] Configure unique indexes (e.g., CardNumber)
- [ ] Configure owned types (e.g., Address)
- [ ] Configure required fields
- [ ] Configure decimal precision for money fields
- [ ] Apply configurations in `OnModelCreating`

### Validation
- [ ] Configuration tests pass ✅
- [ ] Unique indexes are created
- [ ] Owned types work correctly
- [ ] Decimal precision is correct

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 5.3: Implement Card Repository

### TDD Cycle
- [ ] **RED:** Write card repository tests
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Implement `CardRepository` class
- [ ] **GREEN:** Implement all interface methods
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Optimize queries

### Test Implementation
**File: `tests/Authorizer.Infrastructure.Tests/Repositories/CardRepositoryTests.cs`**
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
            CardholderName = "JOHN DOE",
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
    
    [Fact]
    public async Task GetByCardNumberAsync_WhenNotExists_ReturnsNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AuthorizerDbContext(options);
        var repository = new CardRepository(context);

        // Act
        var result = await repository.GetByCardNumberAsync("9999999999999999");

        // Assert
        result.Should().BeNull();
    }
}
```

### Implementation Tasks
- [ ] Create `src/Authorizer.Infrastructure/Repositories/CardRepository.cs`
- [ ] Implement `ICardRepository` interface
- [ ] Implement `GetByCardNumberAsync` method
- [ ] Add any additional required methods
- [ ] Use async/await properly
- [ ] Handle null cases

### Validation
- [ ] All card repository tests pass ✅
- [ ] Queries work correctly
- [ ] Null handling is proper

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 5.4: Implement Account Repository

### TDD Cycle
- [ ] **RED:** Write account repository tests
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Implement `AccountRepository` class
- [ ] **GREEN:** Implement all interface methods
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Optimize queries

### Test Implementation
**File: `tests/Authorizer.Infrastructure.Tests/Repositories/AccountRepositoryTests.cs`**
```csharp
public class AccountRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsAccount()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AuthorizerDbContext(options);
        var accountId = Guid.NewGuid();
        var account = new Account
        {
            AccountId = accountId,
            Status = AccountStatus.Active,
            Balance = 1000.00m
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var repository = new AccountRepository(context);

        // Act
        var result = await repository.GetByIdAsync(accountId);

        // Assert
        result.Should().NotBeNull();
        result!.AccountId.Should().Be(accountId);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AuthorizerDbContext(options);
        var repository = new AccountRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task UpdateBalanceAsync_ShouldUpdateAccountBalance()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AuthorizerDbContext(options);
        var accountId = Guid.NewGuid();
        var account = new Account
        {
            AccountId = accountId,
            Status = AccountStatus.Active,
            Balance = 1000.00m
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var repository = new AccountRepository(context);

        // Act
        await repository.UpdateBalanceAsync(accountId, 500.00m);

        // Assert
        var updated = await context.Accounts.FindAsync(accountId);
        updated!.Balance.Should().Be(500.00m);
    }
}
```

### Implementation Tasks
- [ ] Create `src/Authorizer.Infrastructure/Repositories/AccountRepository.cs`
- [ ] Implement `IAccountRepository` interface
- [ ] Implement `GetByIdAsync` method
- [ ] Implement `UpdateBalanceAsync` method
- [ ] Handle concurrency appropriately

### Validation
- [ ] All account repository tests pass ✅
- [ ] Balance updates work correctly
- [ ] Null handling is proper

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 5.5: Implement Transaction Repository

### TDD Cycle
- [ ] **RED:** Write transaction repository tests
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Implement `TransactionRepository` class
- [ ] **GREEN:** Implement all interface methods
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Optimize queries

### Test Implementation
**File: `tests/Authorizer.Infrastructure.Tests/Repositories/TransactionRepositoryTests.cs`**
```csharp
public class TransactionRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldAddTransaction()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AuthorizerDbContext(options);
        var repository = new TransactionRepository(context);
        
        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            CardNumber = "4111111111111111",
            Amount = 100.00m,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Approved
        };

        // Act
        await repository.AddAsync(transaction);

        // Assert
        var saved = await context.Transactions.FirstOrDefaultAsync();
        saved.Should().NotBeNull();
        saved!.Amount.Should().Be(100.00m);
    }
    
    [Fact]
    public async Task GetRecentTransactionsAsync_ReturnsTransactionsInTimeWindow()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AuthorizerDbContext(options);
        var cardNumber = "4111111111111111";
        
        // Add old transaction (outside window)
        context.Transactions.Add(new Transaction
        {
            TransactionId = Guid.NewGuid(),
            CardNumber = cardNumber,
            Amount = 50.00m,
            Timestamp = DateTime.UtcNow.AddHours(-2),
            Status = TransactionStatus.Approved
        });
        
        // Add recent transactions (inside window)
        context.Transactions.Add(new Transaction
        {
            TransactionId = Guid.NewGuid(),
            CardNumber = cardNumber,
            Amount = 100.00m,
            Timestamp = DateTime.UtcNow.AddMinutes(-30),
            Status = TransactionStatus.Approved
        });
        
        context.Transactions.Add(new Transaction
        {
            TransactionId = Guid.NewGuid(),
            CardNumber = cardNumber,
            Amount = 75.00m,
            Timestamp = DateTime.UtcNow.AddMinutes(-10),
            Status = TransactionStatus.Approved
        });
        
        await context.SaveChangesAsync();

        var repository = new TransactionRepository(context);
        var cutoffTime = DateTime.UtcNow.AddHours(-1);

        // Act
        var result = await repository.GetRecentTransactionsAsync(cardNumber, cutoffTime);

        // Assert
        result.Should().HaveCount(2);
        result.All(t => t.Timestamp >= cutoffTime).Should().BeTrue();
    }
}
```

### Implementation Tasks
- [ ] Create `src/Authorizer.Infrastructure/Repositories/TransactionRepository.cs`
- [ ] Implement `ITransactionRepository` interface
- [ ] Implement `AddAsync` method
- [ ] Implement `GetRecentTransactionsAsync` method with time filtering
- [ ] Add appropriate indexes for query performance

### Validation
- [ ] All transaction repository tests pass ✅
- [ ] Time-based queries work correctly
- [ ] Transactions are saved properly

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 5.6: Create Database Migrations

### Tasks
- [ ] Create initial migration
- [ ] Review generated migration
- [ ] Test migration can be applied
- [ ] Test migration can be rolled back

### Commands
```bash
# Navigate to Infrastructure project
cd src/Authorizer.Infrastructure

# Create initial migration
dotnet ef migrations add InitialCreate --startup-project ../Authorizer.Api

# Review the generated migration file

# Apply migration (in development)
dotnet ef database update --startup-project ../Authorizer.Api
```

### Validation
- [ ] Migration file is created
- [ ] Migration includes all entities
- [ ] Indexes and constraints are included
- [ ] Migration can be applied successfully
- [ ] Database schema matches entities

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Phase 5 Completion Checklist

Before moving to Phase 6, ensure:
- [ ] DbContext is properly configured
- [ ] All entities have configurations
- [ ] Card repository is implemented and tested
- [ ] Account repository is implemented and tested
- [ ] Transaction repository is implemented and tested
- [ ] Database migrations are created
- [ ] All repository tests pass ✅
- [ ] Code follows TDD principles
- [ ] Code is clean and well-documented
- [ ] Queries are optimized
- [ ] Proper async/await usage

### 🎯 FINAL CHECKPOINT: Request comprehensive review of Phase 5

---

## Notes
- Use InMemory database for unit tests
- Use Testcontainers for integration tests
- Configure appropriate indexes for performance
- Handle concurrency appropriately
- Use proper decimal types for money
- Always verify RED before GREEN
- Consider connection resiliency

