# Phase 2: Account Controls (TDD)

## Overview
Implement account validation controls using Test-Driven Development (TDD). This phase establishes account-level validation logic.

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

## Control 2.1: Account Must Be Active

### Step 2.1.1: Create Account Validator

#### TDD Cycle
- [ ] **RED:** Write `AccountValidatorTests.cs` test first
- [ ] **RED:** Run test and verify it FAILS
- [ ] **GREEN:** Create `Account` entity
- [ ] **GREEN:** Create `AccountStatus` enum
- [ ] **GREEN:** Create `IAccountRepository` interface
- [ ] **GREEN:** Create `IAccountValidator` interface
- [ ] **GREEN:** Implement `AccountValidator` class
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Review and optimize

#### Test Implementation
**File: `tests/Authorizer.Application.Tests/Validators/AccountValidatorTests.cs`**
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

#### Implementation Tasks
- [ ] Create `src/Authorizer.Core/Entities/Account.cs`
- [ ] Create `src/Authorizer.Core/Enums/AccountStatus.cs`
- [ ] Create `src/Authorizer.Core/Interfaces/IAccountRepository.cs`
- [ ] Create `src/Authorizer.Core/Interfaces/IAccountValidator.cs`
- [ ] Create `src/Authorizer.Application/Validators/AccountValidator.cs`
- [ ] Implement `ValidateAccountStatusAsync` method

#### Validation
- [ ] All test cases pass ✅
- [ ] Account entity is properly structured
- [ ] All account statuses are defined
- [ ] Validator correctly handles each status

#### ⚠️ CHECKPOINT: Request review before proceeding

---

## Control 2.2: Billing Address Validation

### Step 2.2.1: Add Billing Address Validation

#### TDD Cycle
- [ ] **RED:** Write billing address validation tests
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Create `Address` value object
- [ ] **GREEN:** Implement proper equality comparison
- [ ] **GREEN:** Add `BillingAddress` property to `Account`
- [ ] **GREEN:** Implement `ValidateBillingAddressAsync` method
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Optimize address comparison logic

#### Test Implementation - Match Scenario
**Add to: `tests/Authorizer.Application.Tests/Validators/AccountValidatorTests.cs`**
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
```

#### Test Implementation - Mismatch Scenario
**Add to: `tests/Authorizer.Application.Tests/Validators/AccountValidatorTests.cs`**
```csharp
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

#### Implementation Tasks
- [ ] Create `src/Authorizer.Core/ValueObjects/Address.cs`
- [ ] Implement `Equals` method in `Address`
- [ ] Implement `GetHashCode` method in `Address`
- [ ] Add `BillingAddress` property to `Account` entity
- [ ] Add `ValidateBillingAddressAsync` to `IAccountValidator`
- [ ] Implement `ValidateBillingAddressAsync` in `AccountValidator`

#### Validation
- [ ] Both test scenarios pass ✅
- [ ] Address comparison works correctly
- [ ] Value object properly implements equality
- [ ] All address fields are compared

#### ⚠️ CHECKPOINT: Request review before proceeding

---

### Step 2.2.2: Test Edge Cases for Address Validation

#### TDD Cycle
- [ ] **RED:** Write edge case tests (null, partial matches, etc.)
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Update validation logic to handle edge cases
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Clean up null handling

#### Additional Test Cases
**Add to: `tests/Authorizer.Application.Tests/Validators/AccountValidatorTests.cs`**
```csharp
[Fact]
public async Task ValidateBillingAddress_WhenAccountAddressIsNull_ShouldReturnFailure()
{
    // Arrange
    var accountId = Guid.NewGuid();
    var account = new Account 
    { 
        AccountId = accountId,
        BillingAddress = null
    };
    
    var requestAddress = new Address
    {
        Street = "123 Main St",
        City = "Springfield",
        State = "IL",
        PostalCode = "62701",
        Country = "US"
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
    result.FailureReason.Should().Contain("address");
}

[Fact]
public async Task ValidateBillingAddress_WhenRequestAddressIsNull_ShouldReturnFailure()
{
    // Arrange
    var accountId = Guid.NewGuid();
    var account = new Account 
    { 
        AccountId = accountId,
        BillingAddress = new Address
        {
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "US"
        }
    };
    
    var mockRepo = new Mock<IAccountRepository>();
    mockRepo.Setup(r => r.GetByIdAsync(accountId))
            .ReturnsAsync(account);
    
    var validator = new AccountValidator(mockRepo.Object);

    // Act
    var result = await validator.ValidateBillingAddressAsync(
        accountId, null);

    // Assert
    result.IsValid.Should().BeFalse();
}
```

#### Implementation Tasks
- [ ] Add null checks in `ValidateBillingAddressAsync`
- [ ] Handle null addresses with appropriate error messages
- [ ] Ensure `Address.Equals` handles null properly

#### Validation
- [ ] All edge case tests pass ✅
- [ ] Null handling is robust
- [ ] Error messages are descriptive

#### ⚠️ CHECKPOINT: Request review before proceeding

---

## Phase 2 Completion Checklist

Before moving to Phase 3, ensure:
- [ ] Account entity is properly implemented
- [ ] Account status validation works correctly
- [ ] Address value object is properly implemented
- [ ] Address equality comparison works correctly
- [ ] Billing address validation works for all scenarios
- [ ] Edge cases are handled properly
- [ ] All tests pass ✅
- [ ] Code follows TDD principles
- [ ] Code is clean and well-documented
- [ ] No duplication or code smells

### 🎯 FINAL CHECKPOINT: Request comprehensive review of Phase 2

---

## Notes
- Address value object should be immutable
- Implement proper equality for value objects
- Consider case sensitivity for address fields
- Document any address normalization logic
- Always verify RED before GREEN

