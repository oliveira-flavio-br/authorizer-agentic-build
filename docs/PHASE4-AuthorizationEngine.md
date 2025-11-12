# Phase 4: Authorization Engine (Orchestration)

## Overview
Implement the authorization engine that orchestrates all validators. This phase brings together card, account, and transaction validators into a cohesive authorization workflow.

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

## Step 4.1: Create Authorization Request and Result Models

### TDD Cycle
- [ ] **RED:** Write tests for request/result models
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Create `AuthorizationRequest` class
- [ ] **GREEN:** Create `AuthorizationResult` class
- [ ] **GREEN:** Create `TransactionType` enum
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Review model structure

### Test Implementation
**File: `tests/Authorizer.Core.Tests/Models/AuthorizationRequestTests.cs`**
```csharp
public class AuthorizationRequestTests
{
    [Fact]
    public void AuthorizationRequest_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var request = new AuthorizationRequest
        {
            CardNumber = "4111111111111111",
            Amount = 100.00m,
            MerchantCategoryCode = "5411",
            TransactionType = TransactionType.CardPresent
        };

        // Assert
        request.CardNumber.Should().Be("4111111111111111");
        request.Amount.Should().Be(100.00m);
        request.MerchantCategoryCode.Should().Be("5411");
        request.TransactionType.Should().Be(TransactionType.CardPresent);
    }
}
```

**File: `tests/Authorizer.Core.Tests/Models/AuthorizationResultTests.cs`**
```csharp
public class AuthorizationResultTests
{
    [Fact]
    public void AuthorizationResult_WhenApproved_ShouldHaveAuthCode()
    {
        // Arrange & Act
        var result = AuthorizationResult.Approved(100.00m);

        // Assert
        result.IsApproved.Should().BeTrue();
        result.AuthorizationCode.Should().NotBeNullOrEmpty();
        result.ApprovedAmount.Should().Be(100.00m);
        result.DeclineReason.Should().BeNull();
    }

    [Fact]
    public void AuthorizationResult_WhenDeclined_ShouldHaveReason()
    {
        // Arrange & Act
        var result = AuthorizationResult.Declined("Insufficient funds");

        // Assert
        result.IsApproved.Should().BeFalse();
        result.DeclineReason.Should().Be("Insufficient funds");
        result.AuthorizationCode.Should().BeNull();
    }
}
```

### Implementation Tasks
- [ ] Create `src/Authorizer.Core/Models/AuthorizationRequest.cs`
- [ ] Create `src/Authorizer.Core/Models/AuthorizationResult.cs`
- [ ] Create `src/Authorizer.Core/Enums/TransactionType.cs`
- [ ] Implement static factory methods for `AuthorizationResult`
- [ ] Add all necessary properties

### Validation
- [ ] All model tests pass ✅
- [ ] Models are properly structured
- [ ] Factory methods work correctly

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 4.2: Create Authorization Engine Interface

### TDD Cycle
- [ ] **RED:** Write authorization engine tests
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Create `IAuthorizationEngine` interface
- [ ] **GREEN:** Create `AuthorizationEngine` class
- [ ] **GREEN:** Implement basic authorization workflow
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Optimize orchestration logic

### Test Implementation - Successful Authorization
**File: `tests/Authorizer.Application.Tests/Engine/AuthorizationEngineTests.cs`**
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
}
```

### Test Implementation - Declined Authorization
**Add to: `tests/Authorizer.Application.Tests/Engine/AuthorizationEngineTests.cs`**
```csharp
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
```

### Implementation Tasks
- [ ] Create `src/Authorizer.Core/Interfaces/IAuthorizationEngine.cs`
- [ ] Create `src/Authorizer.Application/Engine/AuthorizationEngine.cs`
- [ ] Implement constructor with validator dependencies
- [ ] Implement `AuthorizeAsync` method
- [ ] Add validation orchestration logic
- [ ] Generate authorization codes for approved transactions

### Validation
- [ ] Both test scenarios pass ✅
- [ ] Authorization workflow is correct
- [ ] Validators are called in proper order
- [ ] Authorization codes are generated

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 4.3: Add Complete Validation Workflow

### TDD Cycle
- [ ] **RED:** Write tests for each failure scenario
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Implement full validation chain
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Optimize validation flow

### Test Implementation - Various Failure Scenarios
**Add to: `tests/Authorizer.Application.Tests/Engine/AuthorizationEngineTests.cs`**
```csharp
[Fact]
public async Task Authorize_WhenCardInactive_ShouldDecline()
{
    // Arrange
    var request = new AuthorizationRequest
    {
        CardNumber = "4111111111111111",
        Amount = 100.00m
    };
    
    var mockCardValidator = new Mock<ICardValidator>();
    mockCardValidator.Setup(v => v.ValidateCardExistsAsync(It.IsAny<string>()))
                    .ReturnsAsync(ValidationResult.Success());
    mockCardValidator.Setup(v => v.ValidateCardStatusAsync(It.IsAny<string>()))
                    .ReturnsAsync(ValidationResult.Failure("Card is inactive"));
    
    var engine = new AuthorizationEngine(
        mockCardValidator.Object,
        Mock.Of<IAccountValidator>(),
        Mock.Of<ITransactionValidator>());

    // Act
    var result = await engine.AuthorizeAsync(request);

    // Assert
    result.IsApproved.Should().BeFalse();
    result.DeclineReason.Should().Contain("inactive");
}

[Fact]
public async Task Authorize_WhenInsufficientFunds_ShouldDecline()
{
    // Arrange
    var request = new AuthorizationRequest
    {
        CardNumber = "4111111111111111",
        Amount = 100.00m
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
    mockTransactionValidator.Setup(v => v.ValidateAvailableBalanceAsync(
        It.IsAny<Guid>(), It.IsAny<decimal>()))
        .ReturnsAsync(ValidationResult.Failure("Insufficient funds"));
    
    var engine = new AuthorizationEngine(
        mockCardValidator.Object,
        mockAccountValidator.Object,
        mockTransactionValidator.Object);

    // Act
    var result = await engine.AuthorizeAsync(request);

    // Assert
    result.IsApproved.Should().BeFalse();
    result.DeclineReason.Should().Contain("Insufficient funds");
}

[Fact]
public async Task Authorize_WhenRateLimitExceeded_ShouldDecline()
{
    // Arrange
    var request = new AuthorizationRequest
    {
        CardNumber = "4111111111111111",
        Amount = 100.00m
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
        .ReturnsAsync(ValidationResult.Failure("Rate limit exceeded"));
    
    var engine = new AuthorizationEngine(
        mockCardValidator.Object,
        mockAccountValidator.Object,
        mockTransactionValidator.Object);

    // Act
    var result = await engine.AuthorizeAsync(request);

    // Assert
    result.IsApproved.Should().BeFalse();
    result.DeclineReason.Should().Contain("Rate limit exceeded");
}
```

### Implementation Tasks
- [ ] Implement card existence check
- [ ] Implement card status check
- [ ] Implement account status check
- [ ] Implement rate limit check
- [ ] Implement MCC validation
- [ ] Implement balance check
- [ ] Ensure validations run in correct order
- [ ] Stop on first validation failure

### Validation
- [ ] All failure scenario tests pass ✅
- [ ] Validation chain works correctly
- [ ] First failure stops the chain
- [ ] Decline reasons are accurate

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 4.4: Add Authorization Code Generation

### TDD Cycle
- [ ] **RED:** Write tests for authorization code generation
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Implement authorization code generation
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Optimize code generation

### Test Implementation
**Add to: `tests/Authorizer.Application.Tests/Engine/AuthorizationEngineTests.cs`**
```csharp
[Fact]
public async Task Authorize_WhenApproved_ShouldGenerateUniqueAuthCode()
{
    // Arrange
    var request = new AuthorizationRequest
    {
        CardNumber = "4111111111111111",
        Amount = 100.00m
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
    var result1 = await engine.AuthorizeAsync(request);
    var result2 = await engine.AuthorizeAsync(request);

    // Assert
    result1.AuthorizationCode.Should().NotBeNullOrEmpty();
    result2.AuthorizationCode.Should().NotBeNullOrEmpty();
    result1.AuthorizationCode.Should().NotBe(result2.AuthorizationCode);
}
```

### Implementation Tasks
- [ ] Implement authorization code generation logic
- [ ] Ensure codes are unique
- [ ] Use appropriate format (e.g., 6-8 alphanumeric characters)
- [ ] Consider using a dedicated service for code generation

### Validation
- [ ] Authorization code test passes ✅
- [ ] Codes are unique
- [ ] Code format is appropriate

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Step 4.5: Add Transaction Recording

### TDD Cycle
- [ ] **RED:** Write tests for transaction recording
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Add transaction repository to engine
- [ ] **GREEN:** Implement transaction recording
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Optimize recording logic

### Test Implementation
**Add to: `tests/Authorizer.Application.Tests/Engine/AuthorizationEngineTests.cs`**
```csharp
[Fact]
public async Task Authorize_WhenApproved_ShouldRecordTransaction()
{
    // Arrange
    var request = new AuthorizationRequest
    {
        CardNumber = "4111111111111111",
        Amount = 100.00m,
        MerchantCategoryCode = "5411"
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
    
    var mockTransactionRepo = new Mock<ITransactionRepository>();
    mockTransactionRepo.Setup(r => r.AddAsync(It.IsAny<Transaction>()))
                      .Returns(Task.CompletedTask);
    
    var engine = new AuthorizationEngine(
        mockCardValidator.Object,
        mockAccountValidator.Object,
        mockTransactionValidator.Object,
        mockTransactionRepo.Object);

    // Act
    var result = await engine.AuthorizeAsync(request);

    // Assert
    result.IsApproved.Should().BeTrue();
    mockTransactionRepo.Verify(r => r.AddAsync(It.Is<Transaction>(t => 
        t.CardNumber == "4111111111111111" &&
        t.Amount == 100.00m &&
        t.Status == TransactionStatus.Approved
    )), Times.Once);
}
```

### Implementation Tasks
- [ ] Add `ITransactionRepository` to engine constructor
- [ ] Create `TransactionStatus` enum (Approved, Declined)
- [ ] Implement transaction creation logic
- [ ] Call repository to save transaction
- [ ] Record both approved and declined transactions

### Validation
- [ ] Transaction recording test passes ✅
- [ ] Transactions are properly created
- [ ] Both approved and declined transactions are recorded

### ⚠️ CHECKPOINT: Request review before proceeding

---

## Phase 4 Completion Checklist

Before moving to Phase 5, ensure:
- [ ] Authorization request model is properly structured
- [ ] Authorization result model has factory methods
- [ ] Authorization engine orchestrates all validators
- [ ] All validation scenarios are tested
- [ ] Authorization codes are generated correctly
- [ ] Transactions are recorded
- [ ] All tests pass ✅
- [ ] Code follows TDD principles
- [ ] Code is clean and well-documented
- [ ] Orchestration logic is efficient

### 🎯 FINAL CHECKPOINT: Request comprehensive review of Phase 4

---

## Notes
- Consider async/await best practices
- Authorization codes should be unique and secure
- Transaction recording should be reliable
- Consider adding retry logic for transient failures
- Always verify RED before GREEN
- Keep the engine focused on orchestration only

