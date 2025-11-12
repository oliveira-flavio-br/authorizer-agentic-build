# Test-Driven Development Guide

## 🎯 TDD Principles for Authorization System

This guide outlines the Test-Driven Development approach for building our payment authorization system.

## The TDD Cycle: Red-Green-Refactor

### 1️⃣ RED - Write a Failing Test
- Write a test for the next bit of functionality
- The test should fail because the functionality doesn't exist yet
- Ensure the test fails for the RIGHT reason

### 2️⃣ GREEN - Make it Pass
- Write the MINIMUM code necessary to make the test pass
- Don't worry about perfection or optimization yet
- Focus on getting to green quickly

### 3️⃣ REFACTOR - Improve the Code
- Clean up the code while keeping tests green
- Remove duplication
- Improve names and structure
- Run tests frequently to ensure nothing breaks

## 🔄 TDD Workflow for Each Feature

```
┌─────────────────────────────────────────┐
│ 1. Understand the Requirement           │
│    - What behavior do we need?          │
│    - What are the edge cases?           │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│ 2. Write Test Cases (Think First!)      │
│    - Happy path                          │
│    - Edge cases                          │
│    - Error scenarios                     │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│ 3. Write ONE Failing Test               │
│    - Start with simplest case           │
│    - Arrange-Act-Assert pattern         │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│ 4. Run Test - Verify it FAILS           │
│    - Test should fail for right reason  │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│ 5. Write Minimal Implementation         │
│    - Just enough to pass the test       │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│ 6. Run Test - Verify it PASSES          │
│    - All tests should be green          │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│ 7. Refactor                              │
│    - Improve code quality               │
│    - Keep tests green                   │
└─────────────────────────────────────────┘
                  ↓
         Repeat for next test
```

## 📝 Test Naming Convention

Use descriptive test names that explain the scenario:

```csharp
[Fact]
public void AuthorizeRequest_WhenCardNotIssued_ShouldDecline()

[Fact]
public void AuthorizeRequest_WhenCardIsActive_ShouldApprove()

[Fact]
public void AuthorizeRequest_WhenCVC2Matches_ShouldApprove()

[Theory]
[InlineData("123", "123", true)]
[InlineData("123", "456", false)]
public void ValidateCVC2_ReturnsExpectedResult(string cardCvc, string requestCvc, bool expected)
```

## 🏗️ Test Structure: Arrange-Act-Assert (AAA)

```csharp
[Fact]
public void AuthorizeRequest_WhenCardIsInactive_ShouldDecline()
{
    // Arrange - Set up test data and dependencies
    var card = new Card 
    { 
        CardNumber = "4111111111111111",
        Status = CardStatus.Inactive
    };
    var request = new AuthorizationRequest 
    { 
        CardNumber = "4111111111111111" 
    };
    var authorizer = new CardAuthorizer();

    // Act - Execute the behavior we're testing
    var result = authorizer.Authorize(request, card);

    // Assert - Verify the outcome
    Assert.False(result.IsApproved);
    Assert.Equal("Card is not active", result.DeclineReason);
}
```

## 🎯 TDD Strategies for Each Control Type

### Card Controls
**Example: "Approve only for cards we have issued"**

1. **Test 1:** Card not found in database → Decline
2. **Test 2:** Card found in database → Proceed to next validation
3. **Test 3:** Null card number → Decline with validation error

### Account Controls
**Example: "Approve only for active accounts"**

1. **Test 1:** Account is active → Proceed
2. **Test 2:** Account is suspended → Decline
3. **Test 3:** Account is closed → Decline

### Transaction Controls
**Example: "x transactions within y time period"**

1. **Test 1:** First transaction → Approve
2. **Test 2:** Within limit → Approve
3. **Test 3:** Exactly at limit → Approve
4. **Test 4:** Over limit → Decline
5. **Test 5:** Expired time window → Reset count and approve

## 🧪 Test Categories

### Unit Tests
- Test individual components in isolation
- Mock dependencies
- Fast execution
- High code coverage

```csharp
public class CardValidatorTests
{
    [Fact]
    public void ValidateCardStatus_ActiveCard_ReturnsValid()
    {
        // Test individual validation logic
    }
}
```

### Integration Tests
- Test interaction between components
- Use real dependencies (but test database)
- Slower but more realistic

```csharp
public class AuthorizationServiceIntegrationTests : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task AuthorizeRequest_WithAllControls_ProcessesCorrectly()
    {
        // Test full authorization flow with database
    }
}
```

### Test Data Builders
Use builder pattern for complex test data:

```csharp
public class AuthorizationRequestBuilder
{
    private string _cardNumber = "4111111111111111";
    private decimal _amount = 100.00m;
    
    public AuthorizationRequestBuilder WithCardNumber(string cardNumber)
    {
        _cardNumber = cardNumber;
        return this;
    }
    
    public AuthorizationRequestBuilder WithAmount(decimal amount)
    {
        _amount = amount;
        return this;
    }
    
    public AuthorizationRequest Build()
    {
        return new AuthorizationRequest
        {
            CardNumber = _cardNumber,
            Amount = _amount
        };
    }
}
```

## 🚀 Test First Mindset

### Before Writing Any Production Code, Ask:
1. What behavior am I implementing?
2. How will I know it works?
3. What test proves it works?
4. What edge cases exist?

### Write Tests for:
- ✅ Happy path (expected success)
- ✅ Validation failures
- ✅ Edge cases (boundary conditions)
- ✅ Null/empty inputs
- ✅ Invalid state transitions
- ✅ Concurrent operations (if applicable)

## 📊 Code Coverage Goals

- **Target:** >90% code coverage
- **Focus:** Cover all logical branches
- **Remember:** 100% coverage ≠ perfect tests
- **Priority:** Meaningful tests over coverage numbers

## 🔍 Common TDD Pitfalls to Avoid

### ❌ Don't Do This:
1. Writing tests after implementation
2. Testing implementation details instead of behavior
3. Making tests depend on other tests
4. Writing tests that are too broad
5. Skipping the refactor step
6. Not running tests frequently

### ✅ Do This Instead:
1. Always write test first
2. Test public interface and observable behavior
3. Each test should be independent
4. Keep tests focused and small
5. Refactor regularly
6. Run tests after every change

## 🎓 TDD Benefits for This Project

1. **Confidence:** Know that controls work correctly
2. **Documentation:** Tests show how the system should behave
3. **Design:** TDD forces good design decisions
4. **Regression:** Catch bugs early when changing code
5. **Speed:** Faster development in the long run

## 📚 Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Framework](https://github.com/moq/moq4) - For mocking dependencies
- [FluentAssertions](https://fluentassertions.com/) - Better assertion syntax
- [Test Data Builders](https://www.natpryce.com/articles/000714.html)

---

**Remember:** The goal is not just to write tests, but to use tests to drive the design of the system!
