# Domain Model

## 📐 Overview

This document defines the domain model for the Payment Authorization System, including entities, enums, value objects, and their relationships.

## Entity Relationship Diagram

```
┌──────────────────┐           ┌──────────────────┐
│     Account      │           │      Card        │
│                  │           │                  │
│ • AccountId (PK) │──────────>│ • CardId (PK)    │
│ • AccountNumber  │  1    *   │ • AccountId (FK) │
│ • Status         │           │ • CardNumber     │
│ • Balance        │           │ • CardholderName │
│ • BillingAddress │           │ • ExpiryDate     │
│ • CreatedAt      │           │ • Status         │
└──────────────────┘           │ • CreatedAt      │
                               └──────────────────┘
                                        │
                                        │ 1
                                        │
                                        │ *
                               ┌────────▼──────────┐
                               │   Transaction     │
                               │                   │
                               │ • TransactionId   │
                               │ • CardId (FK)     │
                               │ • Amount          │
                               │ • MerchantCategory│
                               │ • TransactionType │
                               │ • Timestamp       │
                               │ • IsApproved      │
                               │ • DeclineReason   │
                               └───────────────────┘
```

## Core Entities

### 1. Card

Represents a payment card issued by the institution.

```csharp
public class Card
{
    public Guid CardId { get; set; }
    public Guid AccountId { get; set; }
    public string CardNumber { get; set; }       // e.g., "4111111111111111"
    public string CardholderName { get; set; }    // e.g., "JOHN DOE"
    public DateTime ExpiryDate { get; set; }
    public CardStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public Account Account { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
}
```

**Business Rules:**
- CardNumber must be unique
- CardNumber length: 16 digits (simplified, real-world varies)
- CVC2 is NOT stored (only validated at transaction time)
- Card must belong to an Account

### 2. Account

Represents a customer account that owns cards.

```csharp
public class Account
{
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; }     // e.g., "ACC-12345678"
    public string CustomerName { get; set; }
    public decimal Balance { get; set; }
    public AccountStatus Status { get; set; }
    public Address BillingAddress { get; set; }   // Value Object
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public ICollection<Card> Cards { get; set; }
}
```

**Business Rules:**
- AccountNumber must be unique
- Balance cannot be negative
- BillingAddress required for Card Not Present validations

### 3. Transaction

Represents an authorization request and its outcome.

```csharp
public class Transaction
{
    public Guid TransactionId { get; set; }
    public Guid CardId { get; set; }
    public decimal Amount { get; set; }
    public string MerchantCategoryCode { get; set; }  // e.g., "5411" (Grocery)
    public string MerchantName { get; set; }
    public TransactionType TransactionType { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsApproved { get; set; }
    public string? DeclineReason { get; set; }
    public string? AuthorizationCode { get; set; }    // Generated if approved
    
    // Navigation property
    public Card Card { get; set; }
}
```

**Business Rules:**
- Amount must be positive
- Timestamp is UTC
- DeclineReason required when IsApproved = false
- AuthorizationCode generated for approved transactions

### 4. AuthorizationRequest

Represents an incoming authorization request (not persisted, DTO).

```csharp
public class AuthorizationRequest
{
    public string CardNumber { get; set; }
    public string? CardholderName { get; set; }      // For Card Not Present
    public string? Cvc2 { get; set; }                 // For Card Not Present (never stored)
    public decimal Amount { get; set; }
    public string MerchantCategoryCode { get; set; }
    public string MerchantName { get; set; }
    public TransactionType TransactionType { get; set; }
    public Address? BillingAddress { get; set; }     // For Card Not Present validation
    public DateTime RequestTimestamp { get; set; }
}
```

## Enums

### CardStatus

```csharp
public enum CardStatus
{
    Active = 1,
    Inactive = 2,
    Blocked = 3,
    Expired = 4,
    Lost = 5,
    Stolen = 6
}
```

**Validation Rules:**
- Only `Active` cards can be used for transactions

### AccountStatus

```csharp
public enum AccountStatus
{
    Active = 1,
    Suspended = 2,
    Closed = 3,
    PendingActivation = 4
}
```

**Validation Rules:**
- Only `Active` accounts can process transactions

### TransactionType

```csharp
public enum TransactionType
{
    CardPresent = 1,        // Physical card at terminal
    CardNotPresent = 2,     // Online/phone transactions
    Contactless = 3,        // Tap-to-pay
    ATMWithdrawal = 4
}
```

**Validation Rules:**
- `CardNotPresent` requires CVC2, cardholder name, and billing address validation
- Other types may have different validation rules

### ValidationSeverity

```csharp
public enum ValidationSeverity
{
    Critical = 1,    // Stop processing immediately
    Warning = 2,     // Log but continue
    Info = 3         // Informational only
}
```

## Value Objects

### Address

```csharp
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    
    public override bool Equals(object? obj)
    {
        if (obj is not Address other) return false;
        
        return Street == other.Street &&
               City == other.City &&
               State == other.State &&
               PostalCode == other.PostalCode &&
               Country == other.Country;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Street, City, State, PostalCode, Country);
    }
}
```

**Validation Rules:**
- All fields required
- PostalCode format validated based on Country

### ValidationResult

```csharp
public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string? FailureReason { get; private set; }
    public ValidationSeverity Severity { get; private set; }
    
    public static ValidationResult Success() => new()
    {
        IsValid = true,
        Severity = ValidationSeverity.Info
    };
    
    public static ValidationResult Failure(string reason, ValidationSeverity severity = ValidationSeverity.Critical) => new()
    {
        IsValid = false,
        FailureReason = reason,
        Severity = severity
    };
}
```

### AuthorizationResult

```csharp
public class AuthorizationResult
{
    public bool IsApproved { get; set; }
    public string? DeclineReason { get; set; }
    public string? AuthorizationCode { get; set; }
    public DateTime ProcessedAt { get; set; }
    public decimal AmountAuthorized { get; set; }
    public IList<string> ValidationMessages { get; set; } = new List<string>();
    
    public static AuthorizationResult Approved(decimal amount)
    {
        return new AuthorizationResult
        {
            IsApproved = true,
            AmountAuthorized = amount,
            AuthorizationCode = GenerateAuthorizationCode(),
            ProcessedAt = DateTime.UtcNow
        };
    }
    
    public static AuthorizationResult Declined(string reason)
    {
        return new AuthorizationResult
        {
            IsApproved = false,
            DeclineReason = reason,
            ProcessedAt = DateTime.UtcNow
        };
    }
    
    private static string GenerateAuthorizationCode()
    {
        // Generate 6-digit auth code
        return Random.Shared.Next(100000, 999999).ToString();
    }
}
```

## Interfaces

### IAuthorizationEngine

```csharp
public interface IAuthorizationEngine
{
    Task<AuthorizationResult> AuthorizeAsync(AuthorizationRequest request);
}
```

### ICardValidator

```csharp
public interface ICardValidator
{
    Task<ValidationResult> ValidateCardExistsAsync(string cardNumber);
    Task<ValidationResult> ValidateCardStatusAsync(string cardNumber);
    Task<ValidationResult> ValidateCvc2Async(string cardNumber, string cvc2);
    Task<ValidationResult> ValidateCardholderNameAsync(string cardNumber, string name);
}
```

### IAccountValidator

```csharp
public interface IAccountValidator
{
    Task<ValidationResult> ValidateAccountStatusAsync(Guid accountId);
    Task<ValidationResult> ValidateBillingAddressAsync(Guid accountId, Address address);
}
```

### ITransactionValidator

```csharp
public interface ITransactionValidator
{
    Task<ValidationResult> ValidateRateLimitAsync(string cardNumber, int maxTransactions, TimeSpan timeWindow);
    Task<ValidationResult> ValidateMerchantCategoryAsync(string merchantCategoryCode);
    Task<ValidationResult> ValidateAvailableBalanceAsync(Guid accountId, decimal amount);
}
```

## Configuration Objects

### AuthorizationOptions

```csharp
public class AuthorizationOptions
{
    public RateLimitOptions RateLimit { get; set; }
    public List<string> AllowedMerchantCategories { get; set; }
}

public class RateLimitOptions
{
    public int MaxTransactions { get; set; }
    public int TimeWindowMinutes { get; set; }
}
```

## Domain Events (Future Enhancement)

```csharp
public abstract class DomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}

public class TransactionApprovedEvent : DomainEvent
{
    public Guid TransactionId { get; set; }
    public decimal Amount { get; set; }
}

public class TransactionDeclinedEvent : DomainEvent
{
    public string CardNumber { get; set; }
    public string Reason { get; set; }
}
```

## Merchant Category Codes (Reference)

Common MCC codes for testing:

| MCC  | Description              |
|------|--------------------------|
| 5411 | Grocery Stores           |
| 5812 | Restaurants              |
| 5999 | Miscellaneous Retail     |
| 5912 | Drug Stores              |
| 5541 | Service Stations         |
| 4111 | Local/Suburban Transit   |
| 7011 | Hotels/Motels            |

## Validation Rules Summary

### Card Controls
1. **Card Must Exist:** Query database for card by CardNumber
2. **Card Must Be Active:** Card.Status == CardStatus.Active
3. **CVC2 Match (CNP):** Validate CVC2 against card (external validation service in real world)
4. **Cardholder Name Match (CNP):** Card.CardholderName == Request.CardholderName (case-insensitive)

### Account Controls
5. **Account Must Be Active:** Account.Status == AccountStatus.Active
6. **Billing Address Match (CNP):** Account.BillingAddress == Request.BillingAddress

### Transaction Controls
7. **Rate Limit:** Count transactions in time window < max allowed
8. **MCC Allowed:** Request.MerchantCategoryCode in AllowedMerchantCategories
9. **Sufficient Balance:** Account.Balance >= Request.Amount

## Database Indexes

For optimal performance:

```sql
-- Cards
CREATE INDEX IX_Cards_CardNumber ON Cards(CardNumber);
CREATE INDEX IX_Cards_AccountId ON Cards(AccountId);
CREATE INDEX IX_Cards_Status ON Cards(Status);

-- Accounts
CREATE INDEX IX_Accounts_AccountNumber ON Accounts(AccountNumber);
CREATE INDEX IX_Accounts_Status ON Accounts(Status);

-- Transactions
CREATE INDEX IX_Transactions_CardId ON Transactions(CardId);
CREATE INDEX IX_Transactions_Timestamp ON Transactions(Timestamp);
CREATE INDEX IX_Transactions_IsApproved ON Transactions(IsApproved);
```

---

**Note:** This domain model is designed to be testable and maintainable while representing the core business logic of payment authorization.
