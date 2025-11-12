# TDD Cheatsheet - Quick Reference

## 🚀 Quick Start Commands

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests in watch mode (recommended for TDD!)
dotnet watch test

# Run specific test project
dotnet test tests/Authorizer.Application.Tests

# Run specific test
dotnet test --filter "FullyQualifiedName~CardValidatorTests"

# Run with verbosity
dotnet test --logger "console;verbosity=detailed"
```

### Code Coverage

```bash
# Run tests with coverage
dotnet test /p:CollectCoverage=true

# Generate HTML report
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
reportgenerator -reports:**/coverage.opencover.xml -targetdir:coverage-report
```

### Building

```bash
# Build solution
dotnet build

# Clean and rebuild
dotnet clean && dotnet build
```

---

## 📝 Test Template

### Basic Fact Test

```csharp
[Fact]
public async Task MethodName_StateUnderTest_ExpectedBehavior()
{
    // Arrange
    var input = CreateTestInput();
    var expected = ExpectedValue;
    
    // Act
    var result = await _sut.MethodUnderTest(input);
    
    // Assert
    result.Should().Be(expected);
}
```

### Theory Test (Data-Driven)

```csharp
[Theory]
[InlineData(input1, expected1)]
[InlineData(input2, expected2)]
public async Task MethodName_ReturnsExpectedResult(string input, bool expected)
{
    // Arrange
    
    // Act
    var result = await _sut.MethodUnderTest(input);
    
    // Assert
    result.Should().Be(expected);
}
```

### Async Test

```csharp
[Fact]
public async Task AsyncMethod_Scenario_ExpectedOutcome()
{
    // Arrange
    var input = TestData;
    
    // Act
    var result = await _sut.AsyncMethod(input);
    
    // Assert
    result.Should().NotBeNull();
}
```

---

## 🎭 Moq Cheatsheet

### Basic Setup

```csharp
// Create mock
var mockRepository = new Mock<IRepository>();

// Setup method return
mockRepository
    .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
    .ReturnsAsync(expectedValue);

// Setup with specific parameter
mockRepository
    .Setup(r => r.GetByIdAsync(specificId))
    .ReturnsAsync(expectedValue);

// Setup to throw exception
mockRepository
    .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
    .ThrowsAsync(new Exception("Error"));
```

### Verification

```csharp
// Verify method was called
mockRepository.Verify(
    r => r.GetByIdAsync(It.IsAny<Guid>()), 
    Times.Once);

// Verify method was never called
mockRepository.Verify(
    r => r.DeleteAsync(It.IsAny<Guid>()), 
    Times.Never);

// Verify with specific parameters
mockRepository.Verify(
    r => r.SaveAsync(It.Is<Card>(c => c.Status == CardStatus.Active)), 
    Times.Once);
```

---

## 💎 FluentAssertions Cheatsheet

### Basic Assertions

```csharp
// Equality
result.Should().Be(expected);
result.Should().NotBe(unexpected);

// Null checks
result.Should().BeNull();
result.Should().NotBeNull();

// Boolean
result.Should().BeTrue();
result.Should().BeFalse();

// Strings
result.Should().Contain("substring");
result.Should().StartWith("prefix");
result.Should().EndWith("suffix");
result.Should().BeEmpty();
result.Should().NotBeNullOrWhiteSpace();
```

### Collections

```csharp
// Count
collection.Should().HaveCount(5);
collection.Should().NotBeEmpty();
collection.Should().BeEmpty();

// Contains
collection.Should().Contain(item);
collection.Should().NotContain(item);
collection.Should().ContainSingle(x => x.Status == CardStatus.Active);

// All/Any
collection.Should().OnlyContain(x => x.IsValid);
collection.Should().AllSatisfy(x => x.Should().NotBeNull());
```

### Exceptions

```csharp
// Assert exception is thrown
Action act = () => _sut.ThrowingMethod();
act.Should().Throw<InvalidOperationException>()
   .WithMessage("Expected error message");

// Assert no exception
Action act = () => _sut.SafeMethod();
act.Should().NotThrow();
```

### Objects

```csharp
// Property checks
result.Should().Match<Card>(c => 
    c.Status == CardStatus.Active && 
    c.CardNumber == "4111111111111111");

// Type checks
result.Should().BeOfType<Card>();
result.Should().BeAssignableTo<ICard>();
```

---

## 🔧 Common Test Patterns

### Test Data Builder

```csharp
public class CardBuilder
{
    private string _cardNumber = "4111111111111111";
    private CardStatus _status = CardStatus.Active;

    public CardBuilder WithCardNumber(string number)
    {
        _cardNumber = number;
        return this;
    }

    public CardBuilder Inactive()
    {
        _status = CardStatus.Inactive;
        return this;
    }

    public Card Build() => new()
    {
        CardNumber = _cardNumber,
        Status = _status,
        // ... other properties
    };
}

// Usage
var card = new CardBuilder()
    .WithCardNumber("4222222222222222")
    .Inactive()
    .Build();
```

### Test Fixture

```csharp
public class DatabaseFixture : IDisposable
{
    public AuthorizerDbContext Context { get; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<AuthorizerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        Context = new AuthorizerDbContext(options);
        SeedTestData();
    }

    private void SeedTestData()
    {
        // Add test data
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}

// Usage in test class
public class MyTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public MyTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
}
```

### Object Mother

```csharp
public static class TestData
{
    public static Card ActiveCard() => new()
    {
        CardId = Guid.NewGuid(),
        CardNumber = "4111111111111111",
        CardholderName = "JOHN DOE",
        Status = CardStatus.Active,
        ExpiryDate = DateTime.UtcNow.AddYears(2)
    };

    public static AuthorizationRequest ValidRequest() => new()
    {
        CardNumber = "4111111111111111",
        Amount = 100.00m,
        MerchantCategoryCode = "5411",
        TransactionType = TransactionType.CardPresent
    };
}
```

---

## 🎯 TDD Workflow

### The Red-Green-Refactor Cycle

1. **RED** - Write failing test
   ```bash
   dotnet watch test  # See test fail ❌
   ```

2. **GREEN** - Write minimal implementation
   ```bash
   # Tests pass ✅
   ```

3. **REFACTOR** - Improve code
   ```bash
   # Tests still pass ✅
   ```

### Quick TDD Steps

```
1. Write ONE test for the simplest case
2. Run test → See it FAIL (RED)
3. Write MINIMAL code to pass
4. Run test → See it PASS (GREEN)
5. Refactor if needed
6. Commit
7. Repeat with next test
```

---

## 📋 Test Naming Conventions

### Pattern 1: MethodName_StateUnderTest_ExpectedBehavior

```csharp
[Fact]
public void ValidateCardStatus_InactiveCard_ReturnsFailure()

[Fact]
public void AuthorizeRequest_ValidCard_ReturnsApproved()
```

### Pattern 2: Should_ExpectedBehavior_When_StateUnderTest

```csharp
[Fact]
public void Should_ReturnFailure_When_CardIsInactive()

[Fact]
public void Should_ApproveRequest_When_CardIsValid()
```

### Pattern 3: Given_When_Then (BDD Style)

```csharp
[Fact]
public void Given_InactiveCard_When_Validating_Then_ReturnsFailure()

[Fact]
public void Given_ValidCard_When_Authorizing_Then_ReturnsApproved()
```

---

## 🐛 Debugging Tests

### Debug Single Test

1. Set breakpoint in test
2. Right-click test → Debug Test
3. Or use:
   ```bash
   dotnet test --filter "TestMethodName" --logger "console;verbosity=detailed"
   ```

### View Test Output

```csharp
[Fact]
public void MyTest()
{
    var output = _testOutputHelper; // Inject ITestOutputHelper
    output.WriteLine("Debug message");
    
    // Test code
}
```

---

## ⚡ Performance Tips

### Fast Test Execution

- Use in-memory database for unit tests
- Mock external dependencies
- Avoid Thread.Sleep (use Task.Delay with cancellation)
- Run tests in parallel (xUnit default)

### Test Organization

```
tests/
  ├── Unit/           # Fast, isolated
  ├── Integration/    # Medium speed
  └── E2E/           # Slow, full system
```

---

## 📊 Code Coverage

### Generate Coverage Report

```bash
# Install tool
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Generate HTML report
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report

# Open report
start coverage-report/index.html  # Windows
open coverage-report/index.html   # Mac/Linux
```

---

## 🎨 VS Code Snippets

Add to `.vscode/csharp.json`:

```json
{
  "xUnit Fact Test": {
    "prefix": "testfact",
    "body": [
      "[Fact]",
      "public async Task ${1:MethodName}_${2:Scenario}_${3:ExpectedOutcome}()",
      "{",
      "    // Arrange",
      "    ${4}",
      "    ",
      "    // Act",
      "    ${5}",
      "    ",
      "    // Assert",
      "    ${6}",
      "}"
    ]
  },
  "xUnit Theory Test": {
    "prefix": "testtheory",
    "body": [
      "[Theory]",
      "[InlineData(${1})]",
      "public async Task ${2:MethodName}_ReturnsExpectedResult(${3:parameters})",
      "{",
      "    // Arrange",
      "    ",
      "    // Act",
      "    var result = await _sut.${4:Method}(${5});",
      "    ",
      "    // Assert",
      "    result.Should().${6:Be}(${7:expected});",
      "}"
    ]
  }
}
```

---

## 🔥 Common Mistakes to Avoid

❌ **Don't write tests after implementation**
✅ Write tests FIRST

❌ **Don't test private methods**
✅ Test public interface

❌ **Don't make tests depend on each other**
✅ Each test is independent

❌ **Don't test multiple things in one test**
✅ One assertion per test (generally)

❌ **Don't use production database**
✅ Use in-memory or test containers

---

## 📚 Quick Links

- [xUnit Docs](https://xunit.net/)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [FluentAssertions Docs](https://fluentassertions.com/introduction)
- [TDD Guide](./TDD-GUIDE.md)
- [Testing Strategy](./TESTING-STRATEGY.md)

---

**Keep this cheatsheet handy while practicing TDD! 🚀**
