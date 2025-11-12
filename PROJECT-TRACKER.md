# Payment Authorization System - Project Tracker

## 🎯 Project Goals

Build a payment authorization system that applies various controls to approve or decline authorization requests using **Test-Driven Development (TDD)** methodology.

### Technology Stack
- **.NET 8** - Application framework
- **C#** - Programming language
- **PostgreSQL** - Database
- **Entity Framework Core** - ORM
- **xUnit** - Testing framework
- **.NET Aspire** - Orchestration and observability

## 📋 Project Status

**Current Phase:** Setup & Planning
**Started:** November 12, 2025
**Last Updated:** November 12, 2025

---

## ✅ Implementation Checklist

### Phase 0: Project Setup
- [ ] Initialize .NET Aspire project structure
- [ ] Setup PostgreSQL with Docker/Aspire
- [ ] Configure Entity Framework Core
- [ ] Setup xUnit test projects
- [ ] Create domain model structure
- [ ] Configure CI/CD basics (if needed)

### Phase 1: Card Controls (TDD)
- [ ] **Control 1.1:** Approve only for cards we have issued
  - [ ] Write failing tests
  - [ ] Implement card existence validation
  - [ ] Refactor and verify
  
- [ ] **Control 1.2:** Approve only for active cards
  - [ ] Write failing tests
  - [ ] Implement card status validation
  - [ ] Refactor and verify

- [ ] **Control 1.3:** Approve only for matching CVC2 (Card Not Present)
  - [ ] Write failing tests
  - [ ] Implement CVC2 validation logic
  - [ ] Refactor and verify

- [ ] **Control 1.4:** Approve only for matching cardholder name (Card Not Present)
  - [ ] Write failing tests
  - [ ] Implement cardholder name validation
  - [ ] Refactor and verify

### Phase 2: Account Controls (TDD)
- [ ] **Control 2.1:** Approve only for active accounts
  - [ ] Write failing tests
  - [ ] Implement account status validation
  - [ ] Refactor and verify

- [ ] **Control 2.2:** Approve only for matching billing address (Card Not Present)
  - [ ] Write failing tests
  - [ ] Implement billing address validation
  - [ ] Refactor and verify

### Phase 3: Transaction Controls (TDD)
- [ ] **Control 3.1:** Rate limiting - x transactions within y time period
  - [ ] Write failing tests
  - [ ] Implement transaction rate limiting logic
  - [ ] Refactor and verify

- [ ] **Control 3.2:** Approved Merchant Category Codes (MCC)
  - [ ] Write failing tests
  - [ ] Implement MCC validation
  - [ ] Refactor and verify

- [ ] **Control 3.3:** Transaction amount < available balance
  - [ ] Write failing tests
  - [ ] Implement balance validation
  - [ ] Refactor and verify

### Phase 4: Integration & Polish
- [ ] Integration tests across all controls
- [ ] Performance testing
- [ ] Code coverage analysis (aim for >90%)
- [ ] Documentation updates
- [ ] Refactoring pass

---

## 📊 Progress Metrics

| Phase | Tasks Complete | Tasks Total | % Complete |
|-------|---------------|-------------|------------|
| Phase 0 | 0 | 6 | 0% |
| Phase 1 | 0 | 4 | 0% |
| Phase 2 | 0 | 2 | 0% |
| Phase 3 | 0 | 3 | 0% |
| Phase 4 | 0 | 4 | 0% |
| **Total** | **0** | **19** | **0%** |

---

## 🎓 Key Learnings

_Document key insights, challenges, and solutions discovered during implementation_

### What Worked Well
- TBD

### Challenges Overcome
- TBD

### Agentic Coding Insights
- TBD

---

## 📚 Related Documents
- [TDD Guide](./docs/TDD-GUIDE.md) - Test-Driven Development approach for this project
- [TDD Cheatsheet](./docs/TDD-CHEATSHEET.md) - Quick reference for TDD commands and patterns ⚡
- [Architecture Overview](./docs/ARCHITECTURE.md) - System architecture and design decisions
- [Domain Model](./docs/DOMAIN-MODEL.md) - Core domain entities and relationships
- [Implementation Plan](./docs/IMPLEMENTATION-PLAN.md) - Detailed step-by-step implementation guide
- [Testing Strategy](./docs/TESTING-STRATEGY.md) - Comprehensive testing approach

---

## 🔗 References
- [ISO 8583 Specification](https://en.wikipedia.org/wiki/ISO_8583)
- [Mastercard Authorization Manual](https://developer.mastercard.com/)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
