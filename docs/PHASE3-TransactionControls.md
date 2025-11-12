# Phase 3: Transaction Controls (TDD)

## Overview
Implement transaction validation controls using Test-Driven Development (TDD). This phase establishes transaction-level validation logic including rate limiting, merchant categories, and balance checks.

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

## Control 3.1: Rate Limiting

### Step 3.1.1: Implement Rate Limit Validation

#### TDD Cycle
- [ ] **RED:** Write rate limiting tests
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Create `Transaction` entity
- [ ] **GREEN:** Create `ITransactionRepository` interface
- [ ] **GREEN:** Create `ITransactionValidator` interface
- [ ] **GREEN:** Implement `TransactionValidator` class
- [ ] **GREEN:** Implement `ValidateRateLimitAsync` method
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Optimize rate limit logic

#### Test Implementation - Under Limit
**File: `tests/Authorizer.Application.Tests/Validators/TransactionValidatorTests.cs`**
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
}
```

#### Test Implementation - Over Limit
**Add to: `tests/Authorizer.Application.Tests/Validators/TransactionValidatorTests.cs`**
```csharp
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
```

#### Implementation Tasks
- [ ] Create `src/Authorizer.Core/Entities/Transaction.cs`
- [ ] Create `src/Authorizer.Core/Interfaces/ITransactionRepository.cs`
- [ ] Create `src/Authorizer.Core/Interfaces/ITransactionValidator.cs`
- [ ] Create `src/Authorizer.Application/Validators/TransactionValidator.cs`
- [ ] Implement `GetRecentTransactionsAsync` in repository interface
- [ ] Implement `ValidateRateLimitAsync` method
- [ ] Add proper timestamp filtering logic

#### Validation
- [ ] Both test scenarios pass ✅
- [ ] Rate limiting logic is correct
- [ ] Time window calculation is accurate
- [ ] Error messages are clear

#### ⚠️ CHECKPOINT: Request review before proceeding

---

### Step 3.1.2: Test Rate Limit Edge Cases

#### TDD Cycle
- [ ] **RED:** Write edge case tests (boundary conditions)
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Update rate limit logic to handle edges
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Optimize time window logic

#### Additional Test Cases
**Add to: `tests/Authorizer.Application.Tests/Validators/TransactionValidatorTests.cs`**
```csharp
[Fact]
public async Task ValidateRateLimit_WhenExactlyAtLimit_ShouldReturnFailure()
{
    // Arrange
    var cardNumber = "4111111111111111";
    var existingTransactions = Enumerable.Range(0, 3)
        .Select(_ => new Transaction 
        { 
            CardNumber = cardNumber, 
            Timestamp = DateTime.UtcNow.AddMinutes(-10) 
        })
        .ToList();
    
    var mockRepo = new Mock<ITransactionRepository>();
    mockRepo.Setup(r => r.GetRecentTransactionsAsync(
        cardNumber, It.IsAny<DateTime>()))
        .ReturnsAsync(existingTransactions);
    
    var validator = new TransactionValidator(mockRepo.Object);

    // Act
    var result = await validator.ValidateRateLimitAsync(
        cardNumber, maxTransactions: 3, timeWindow: TimeSpan.FromHours(1));

    // Assert
    result.IsValid.Should().BeFalse();
}

[Fact]
public async Task ValidateRateLimit_WhenTransactionsOutsideWindow_ShouldReturnSuccess()
{
    // Arrange
    var cardNumber = "4111111111111111";
    var existingTransactions = Enumerable.Range(0, 10)
        .Select(_ => new Transaction 
        { 
            CardNumber = cardNumber, 
            Timestamp = DateTime.UtcNow.AddHours(-2) // Outside 1-hour window
        })
        .ToList();
    
    var mockRepo = new Mock<ITransactionRepository>();
    mockRepo.Setup(r => r.GetRecentTransactionsAsync(
        cardNumber, It.IsAny<DateTime>()))
        .ReturnsAsync(new List<Transaction>()); // Repository filters by time
    
    var validator = new TransactionValidator(mockRepo.Object);

    // Act
    var result = await validator.ValidateRateLimitAsync(
        cardNumber, maxTransactions: 5, timeWindow: TimeSpan.FromHours(1));

    // Assert
    result.IsValid.Should().BeTrue();
}
```

#### Implementation Tasks
- [ ] Handle exact limit boundary (>= vs >)
- [ ] Ensure time window filtering is accurate
- [ ] Test with various time windows

#### Validation
- [ ] All edge case tests pass ✅
- [ ] Boundary conditions are correct
- [ ] Time calculations are accurate

#### ⚠️ CHECKPOINT: Request review before proceeding

---

## Control 3.2: Merchant Category Code Validation

### Step 3.2.1: Add MCC Validation

#### TDD Cycle
- [ ] **RED:** Write MCC validation tests
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Create `AuthorizationOptions` configuration class
- [ ] **GREEN:** Implement `ValidateMerchantCategoryAsync` method
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Clean up configuration handling

#### Test Implementation
**Add to: `tests/Authorizer.Application.Tests/Validators/TransactionValidatorTests.cs`**
```csharp
[Theory]
[InlineData("5411", true)]  // Grocery stores - Allowed
[InlineData("5812", true)]  // Restaurants - Allowed
[InlineData("9999", false)] // Unknown - Not allowed
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

#### Implementation Tasks
- [ ] Create `src/Authorizer.Core/Configuration/AuthorizationOptions.cs`
- [ ] Add `AllowedMerchantCategories` property
- [ ] Add `ValidateMerchantCategoryAsync` to `ITransactionValidator`
- [ ] Update `TransactionValidator` constructor to accept options
- [ ] Implement `ValidateMerchantCategoryAsync` method

#### Validation
- [ ] All test cases pass ✅
- [ ] Configuration is properly injected
- [ ] MCC validation logic is correct
- [ ] Handles unknown MCCs properly

#### ⚠️ CHECKPOINT: Request review before proceeding

---

### Step 3.2.2: Test MCC Edge Cases

#### TDD Cycle
- [ ] **RED:** Write edge case tests
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Update validation to handle edges
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Optimize MCC lookup

#### Additional Test Cases
**Add to: `tests/Authorizer.Application.Tests/Validators/TransactionValidatorTests.cs`**
```csharp
[Fact]
public async Task ValidateMerchantCategory_WhenMccIsNull_ShouldReturnFailure()
{
    // Arrange
    var options = Options.Create(new AuthorizationOptions
    {
        AllowedMerchantCategories = new List<string> { "5411" }
    });
    
    var validator = new TransactionValidator(
        Mock.Of<ITransactionRepository>(), options);

    // Act
    var result = await validator.ValidateMerchantCategoryAsync(null);

    // Assert
    result.IsValid.Should().BeFalse();
}

[Fact]
public async Task ValidateMerchantCategory_WhenNoAllowedCategories_ShouldReturnFailure()
{
    // Arrange
    var options = Options.Create(new AuthorizationOptions
    {
        AllowedMerchantCategories = new List<string>()
    });
    
    var validator = new TransactionValidator(
        Mock.Of<ITransactionRepository>(), options);

    // Act
    var result = await validator.ValidateMerchantCategoryAsync("5411");

    // Assert
    result.IsValid.Should().BeFalse();
}
```

#### Implementation Tasks
- [ ] Handle null/empty MCC
- [ ] Handle empty allowed categories list
- [ ] Add appropriate error messages

#### Validation
- [ ] All edge case tests pass ✅
- [ ] Null handling is robust
- [ ] Error messages are descriptive

#### ⚠️ CHECKPOINT: Request review before proceeding

---

## Control 3.3: Available Balance Validation

### Step 3.3.1: Add Balance Validation

#### TDD Cycle
- [ ] **RED:** Write balance validation tests
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Add `Balance` property to `Account` entity
- [ ] **GREEN:** Implement `ValidateAvailableBalanceAsync` method
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Optimize balance comparison

#### Test Implementation
**Add to: `tests/Authorizer.Application.Tests/Validators/TransactionValidatorTests.cs`**
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

#### Implementation Tasks
- [ ] Add `Balance` property to `Account` entity
- [ ] Update `TransactionValidator` constructor to accept `IAccountRepository`
- [ ] Add `ValidateAvailableBalanceAsync` to `ITransactionValidator`
- [ ] Implement `ValidateAvailableBalanceAsync` method
- [ ] Handle decimal comparison correctly

#### Validation
- [ ] All test cases pass ✅
- [ ] Balance comparison is accurate
- [ ] Decimal precision is handled correctly
- [ ] Boundary conditions are correct

#### ⚠️ CHECKPOINT: Request review before proceeding

---

### Step 3.3.2: Test Balance Edge Cases

#### TDD Cycle
- [ ] **RED:** Write edge case tests
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Update validation to handle edges
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Clean up balance logic

#### Additional Test Cases
**Add to: `tests/Authorizer.Application.Tests/Validators/TransactionValidatorTests.cs`**
```csharp
[Fact]
public async Task ValidateAvailableBalance_WhenNegativeAmount_ShouldReturnFailure()
{
    // Arrange
    var accountId = Guid.NewGuid();
    var account = new Account { AccountId = accountId, Balance = 1000m };
    
    var mockAccountRepo = new Mock<IAccountRepository>();
    mockAccountRepo.Setup(r => r.GetByIdAsync(accountId))
                   .ReturnsAsync(account);
    
    var validator = new TransactionValidator(
        Mock.Of<ITransactionRepository>(),
        mockAccountRepo.Object);

    // Act
    var result = await validator.ValidateAvailableBalanceAsync(accountId, -100m);

    // Assert
    result.IsValid.Should().BeFalse();
}

[Fact]
public async Task ValidateAvailableBalance_WhenZeroAmount_ShouldReturnFailure()
{
    // Arrange
    var accountId = Guid.NewGuid();
    var account = new Account { AccountId = accountId, Balance = 1000m };
    
    var mockAccountRepo = new Mock<IAccountRepository>();
    mockAccountRepo.Setup(r => r.GetByIdAsync(accountId))
                   .ReturnsAsync(account);
    
    var validator = new TransactionValidator(
        Mock.Of<ITransactionRepository>(),
        mockAccountRepo.Object);

    // Act
    var result = await validator.ValidateAvailableBalanceAsync(accountId, 0m);

    // Assert
    result.IsValid.Should().BeFalse();
}
```

#### Implementation Tasks
- [ ] Validate transaction amount is positive
- [ ] Handle zero amount transactions
- [ ] Add appropriate error messages

#### Validation
- [ ] All edge case tests pass ✅
- [ ] Amount validation is correct
- [ ] Error messages are clear

#### ⚠️ CHECKPOINT: Request review before proceeding

---

## Phase 3 Completion Checklist

Before moving to Phase 4, ensure:
- [ ] Transaction entity is properly implemented
- [ ] Rate limiting validation works correctly
- [ ] Rate limit edge cases are handled
- [ ] MCC validation works correctly
- [ ] MCC edge cases are handled
- [ ] Balance validation works correctly
- [ ] Balance edge cases are handled
- [ ] All tests pass ✅
- [ ] Code follows TDD principles
- [ ] Code is clean and well-documented
- [ ] No duplication or code smells
- [ ] Configuration is properly structured

### 🎯 FINAL CHECKPOINT: Request comprehensive review of Phase 3

---

## Notes
- Consider configurable rate limit parameters
- MCC codes should be standardized (ISO 18245)
- Balance checks should be atomic in production
- Always verify RED before GREEN
- Keep validators focused and single-purpose

