# Phase 1: Card Controls (TDD)

## Overview
Implement card validation controls using Test-Driven Development (TDD). This phase establishes the core card validation logic.

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

## Control 1.1: Card Must Exist

### Step 1.1.1: Create Domain Entities (Core)

#### TDD Cycle
- [ ] **RED:** Write `CardTests.cs` test first
- [ ] **RED:** Run test and verify it FAILS (class doesn't exist yet)
- [ ] **GREEN:** Create `Card` entity with minimal properties
- [ ] **GREEN:** Create `CardStatus` enum
- [ ] **GREEN:** Run test and verify it PASSES
- [ ] **REFACTOR:** Review and improve if needed

#### Test Implementation
**File: `tests/Authorizer.Core.Tests/Entities/CardTests.cs`**
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

#### Implementation Tasks
- [ ] Create `src/Authorizer.Core/Entities/Card.cs`
- [ ] Create `src/Authorizer.Core/Enums/CardStatus.cs`
- [ ] Ensure all properties are correctly defined

#### Validation
- [ ] Test passes ✅
- [ ] Code is clean and follows conventions
- [ ] Entity follows domain-driven design principles

#### ⚠️ CHECKPOINT: Request review before proceeding

---

### Step 1.1.2: Create Card Repository Interface

#### TDD Cycle
- [ ] **RED:** Write `CardValidatorTests.cs` test first
- [ ] **RED:** Run test and verify it FAILS
- [ ] **GREEN:** Create `ICardRepository` interface
- [ ] **GREEN:** Create `ValidationResult` value object
- [ ] **GREEN:** Create `ICardValidator` interface
- [ ] **GREEN:** Implement `CardValidator` class
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Clean up and optimize

#### Test Implementation
**File: `tests/Authorizer.Application.Tests/Validators/CardValidatorTests.cs`**
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

#### Implementation Tasks
- [ ] Create `src/Authorizer.Core/Interfaces/ICardRepository.cs`
- [ ] Create `src/Authorizer.Core/ValueObjects/ValidationResult.cs`
- [ ] Create `src/Authorizer.Core/Interfaces/ICardValidator.cs`
- [ ] Create `src/Authorizer.Application/Validators/CardValidator.cs`
- [ ] Implement `ValidateCardExistsAsync` method

#### Validation
- [ ] Both tests pass ✅
- [ ] Interfaces are in Core layer
- [ ] Implementation is in Application layer
- [ ] Proper separation of concerns

#### ⚠️ CHECKPOINT: Request review before proceeding

---

## Control 1.2: Card Must Be Active

### Step 1.2.1: Add Card Status Validation

#### TDD Cycle
- [ ] **RED:** Add test for card status validation
- [ ] **RED:** Run test and verify it FAILS
- [ ] **GREEN:** Implement `ValidateCardStatusAsync` method
- [ ] **GREEN:** Run test and verify it PASSES
- [ ] **REFACTOR:** Review and optimize

#### Test Implementation
**Add to: `tests/Authorizer.Application.Tests/Validators/CardValidatorTests.cs`**
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

#### Implementation Tasks
- [ ] Add all `CardStatus` enum values (Active, Inactive, Blocked, Expired)
- [ ] Add `ValidateCardStatusAsync` method to `ICardValidator`
- [ ] Implement `ValidateCardStatusAsync` in `CardValidator`
- [ ] Handle all status cases correctly

#### Validation
- [ ] All test cases pass ✅
- [ ] Status validation logic is correct
- [ ] Error messages are clear and descriptive

#### ⚠️ CHECKPOINT: Request review before proceeding

---

## Control 1.3: CVC2 Validation (Card Not Present)

### Step 1.3.1: Add CVC2 Validation

#### TDD Cycle
- [ ] **RED:** Write CVC2 validation tests
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Create `ICvc2ValidationService` interface
- [ ] **GREEN:** Implement CVC2 validation service
- [ ] **GREEN:** Update `CardValidator` to use the service
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Clean up implementation

#### Test Implementation
**Add to: `tests/Authorizer.Application.Tests/Validators/CardValidatorTests.cs`**
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

#### Implementation Tasks
- [ ] Create `src/Authorizer.Core/Interfaces/ICvc2ValidationService.cs`
- [ ] Create `src/Authorizer.Application/Services/Cvc2ValidationService.cs`
- [ ] Update `CardValidator` constructor to accept `ICvc2ValidationService`
- [ ] Add `ValidateCvc2Async` method to `ICardValidator`
- [ ] Implement `ValidateCvc2Async` in `CardValidator`

#### Validation
- [ ] All test cases pass ✅
- [ ] CVC2 validation works for all scenarios
- [ ] Service is properly injected

#### ⚠️ CHECKPOINT: Request review before proceeding

---

## Control 1.4: Cardholder Name Validation (Card Not Present)

### Step 1.4.1: Add Name Validation

#### TDD Cycle
- [ ] **RED:** Write cardholder name validation tests
- [ ] **RED:** Run tests and verify they FAIL
- [ ] **GREEN:** Implement name validation logic
- [ ] **GREEN:** Run tests and verify they PASS
- [ ] **REFACTOR:** Optimize comparison logic

#### Test Implementation
**Add to: `tests/Authorizer.Application.Tests/Validators/CardValidatorTests.cs`**
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

#### Implementation Tasks
- [ ] Add `ValidateCardholderNameAsync` to `ICardValidator`
- [ ] Implement `ValidateCardholderNameAsync` in `CardValidator`
- [ ] Ensure case-insensitive comparison
- [ ] Handle null/empty name scenarios

#### Validation
- [ ] All test cases pass ✅
- [ ] Case-insensitive comparison works correctly
- [ ] Edge cases are handled properly

#### ⚠️ CHECKPOINT: Request review before proceeding

---

## Phase 1 Completion Checklist

Before moving to Phase 2, ensure:
- [ ] All card entity tests pass
- [ ] Card validator tests all pass
- [ ] Card existence validation works
- [ ] Card status validation works
- [ ] CVC2 validation works
- [ ] Cardholder name validation works
- [ ] All code follows TDD principles
- [ ] Code is clean and well-documented
- [ ] No code smells or duplication

### 🎯 FINAL CHECKPOINT: Request comprehensive review of Phase 1

---

## Notes
- Always start with RED (failing test)
- Don't skip the FAIL verification step
- Write minimal code to pass tests
- Refactor only after tests pass
- Keep tests simple and focused

