# MASTER BUILD PROMPT — Lightweight Personal Finance Manager

## 1. Project Overview

Build a lightweight, secure, maintainable personal finance management application using **ASP.NET Core 9 and C#**.

The application is intended for personal use. Its primary purpose is to manage bank accounts and bank cards, record deposits and withdrawals, track balances, and provide useful financial reports and charts.

The application must prioritize:

1. Minimal resource consumption.
2. Simple architecture and low maintenance costs.
3. Fast startup and response times.
4. Accurate financial calculations and transaction records.
5. A clean, responsive user interface.
6. Local deployment without requiring external cloud services.

Do not overengineer this project. Implement only features that directly support the requirements below.

## 2. Mandatory Technology Stack

Use the following stack:

- Framework: ASP.NET Core 9 MVC.
- Language: C#.
- UI: Razor Views, HTML5, CSS3, and vanilla JavaScript.
- Styling: Bootstrap 5, using locally served static assets.
- Database: SQLite.
- ORM: Entity Framework Core 9 with the SQLite provider.
- Charts: Chart.js, using a locally hosted library.
- Validation: ASP.NET Core model validation and server-side business validation.
- Logging: Built-in Microsoft.Extensions.Logging.
- Testing: xUnit.
- Deployment: Self-contained or framework-dependent deployment, according to the target operating system.

Use built-in ASP.NET Core features whenever possible.

Do not introduce a frontend build pipeline or unnecessary infrastructure.

### Strictly Prohibited Unless Explicitly Approved

- React, Angular, Vue, Blazor, Node.js, and npm.
- Microservices or distributed architecture.
- Docker as a mandatory runtime dependency.
- Redis, message brokers, and background workers without a demonstrated need.
- MediatR, AutoMapper, CQRS frameworks, or elaborate generic repository patterns.
- Third-party authentication providers.
- Cloud databases or external financial APIs.
- Unnecessary NuGet packages.

The application must function without an internet connection after installation.

## 3. Application Architecture

Use a simple, maintainable layered structure:

- **Web:** MVC controllers, Razor views, filters, and application configuration.
- **Core:** Domain entities, financial rules, and business services.
- **Infrastructure:** EF Core DbContext, SQLite configuration, and persistence.
- **Tests:** Unit tests for financial calculations and transaction rules.

Keep the project structure proportional to its size. Avoid excessive interfaces, abstractions, and files that add complexity without practical value.

Use dependency injection provided by ASP.NET Core.

Use asynchronous database operations where appropriate.

Keep controllers thin. Put financial business rules in dedicated services.

## 4. Core Features

### 4.1 Dashboard

Create a dashboard showing:

- Total balance across all active accounts.
- Total deposits for the selected period.
- Total withdrawals for the selected period.
- Net cash flow for the selected period.
- Account and card summaries.
- Recent transactions.
- Monthly deposits versus withdrawals chart.
- Expense distribution chart, where expense categories are available.

Allow the user to select common reporting periods, such as this month, last month, this year, and a custom date range.

All dashboard figures must be calculated from actual database records. Never use fabricated or hardcoded financial figures.

### 4.2 Bank Account Management

Implement complete CRUD operations for bank accounts.

Each account should contain:

- Id.
- Bank name.
- Account title or nickname.
- Account type.
- Account number, optionally.
- IBAN, optionally.
- Currency code.
- Initial balance.
- Creation date.
- Active/inactive status.
- Optional notes.

The initial balance represents the opening balance before any recorded transactions.

Account numbers and IBANs must be treated as sensitive information and masked in the user interface by default.

Support a user-defined account nickname, such as "Salary Account" or "Daily Expenses".

Do not assume every account uses the same currency. Never combine different currencies into one monetary total unless an explicit exchange-rate and conversion feature is implemented. For the initial version, group financial summaries by currency.

### 4.3 Bank Card Management

Implement bank card management.

Each card should contain:

- Id.
- Associated bank account.
- Bank name, inherited or displayed from the associated account where practical.
- Card nickname.
- Last four digits.
- Optional expiration month and year.
- Active/inactive status.
- Optional notes.

A card is associated with an existing bank account.

**Important:** A bank card is not a separate financial account by default. Transactions recorded against a card must affect its associated account balance, not create a duplicate balance.

Never store card PINs, CVVs, online banking passwords, or other unnecessary authentication secrets.

Avoid storing the full card number unless absolutely necessary.

### 4.4 Transaction Management

Implement deposit and withdrawal recording.

Each transaction must contain:

- Id.
- AccountId.
- Transaction type: Deposit or Withdrawal.
- Amount.
- Transaction date.
- Optional transaction category.
- Description.
- Optional reference number.
- Optional associated card.
- Creation timestamp.

Supported operations:

- Create a transaction.
- View transaction details.
- Edit a transaction when permitted.
- Delete a transaction when permitted.
- Search transactions.
- Filter by account, type, category, and date range.
- Sort and paginate transaction lists.

Transaction amounts must always be positive. The transaction type determines whether the amount increases or decreases the balance.

Calculate the account balance as:

Opening Balance + Deposits - Withdrawals.

Never allow floating-point arithmetic for money. Use decimal in C# and a consistent, documented SQLite storage strategy that preserves exact monetary values, such as storing amounts in integer minor units when the currency's precision is known.

For currencies without minor units, respect their currency-specific precision. Avoid silently rounding financial amounts.

Validate all amounts and dates on the server.

Prevent invalid transactions, including zero or negative amounts.

Do not permit a transaction to be associated with a card belonging to a different account.

### 4.5 Transfers Between Accounts

Support transfers between two accounts belonging to the same currency.

A transfer must be recorded as one logical operation that creates two linked ledger entries:

- Withdrawal from the source account.
- Deposit into the destination account.

Both entries must be committed in a single database transaction.

Prevent transfers between different currencies in the initial version.

Transfers must not be counted as external income or expenses in cash-flow reports.

Allow the user to identify linked transfer entries and navigate between them.

If transfers are not implemented in the current phase, clearly document this limitation and do not simulate transfers using unrelated deposits and withdrawals.

### 4.6 Categories

Provide simple transaction categories.

Examples:

- Salary.
- Food.
- Transportation.
- Bills.
- Shopping.
- Healthcare.
- Entertainment.
- Other.

Allow the user to create, edit, and deactivate custom categories.

Support separate category usage for deposits and withdrawals where appropriate.

Categories are optional, but transactions should be categorized when useful for reporting.

Do not build a complex accounting chart of accounts.

### 4.7 Reports and Charts

Create a dedicated reports section.

Include:

**Financial summary**
- Opening balance.
- Total deposits.
- Total withdrawals.
- Net cash flow.
- Closing balance.

**Monthly report**
- Deposits by month.
- Withdrawals by month.
- Net cash flow by month.

**Account report**
- Balance for each account.
- Transaction history for an individual account.
- Deposits and withdrawals by account.

**Category report**
- Withdrawals grouped by category.
- Category distribution for a selected period.

**Transaction report**
- Detailed transaction list.
- Date, account, type, category, description, and amount.
- Filtering by date range and account.

Requirements:

- Use Chart.js for simple, useful visualizations.
- Use server-side queries and aggregation for report calculations.
- Return only the data needed to render the charts.
- Provide a table alongside charts wherever practical.
- Display empty states when no data exists.
- Ensure all report totals reconcile with the underlying ledger.
- Exclude internal transfers from external income and expense totals.
- Clearly distinguish cash flow from account balance.

Do not implement PDF or Excel export unless requested in a later phase.

### 4.8 Search and Filtering

Provide fast filtering for transaction history by:

- Date range.
- Bank account.
- Transaction type.
- Category.
- Description or reference number.

Use database-side filtering and ordering.

Implement pagination for transaction lists. Do not load the entire transaction history into memory.

## 5. User Interface

Build a clean, lightweight, responsive interface.

Requirements:

- Simple dashboard.
- Sidebar or compact navigation.
- Accounts.
- Cards.
- Transactions.
- Transfers.
- Reports.
- Categories.
- Settings.

Use accessible forms, clear validation messages, confirmation dialogs for destructive actions, and consistent number formatting.

Use Persian (fa-IR) and right-to-left layout as the initial UI language.

Use localized date display where appropriate. Store dates consistently and distinguish a transaction's calendar date from its creation timestamp.

For the initial release, support a configurable currency display and a default currency setting. Never hardcode the assumption that all currencies are interchangeable.

Keep JavaScript minimal. Use ordinary HTML forms and MVC post actions for standard operations; use AJAX only where it materially improves usability.

Do not introduce a client-side SPA.

## 6. Security and Data Integrity

This application stores personal financial information. Security and correctness are mandatory.

- Use authentication for access to financial data.
- For a single-user installation, implement simple local authentication without external identity providers.
- Store passwords using ASP.NET Core's established password hashing mechanisms.
- Protect state-changing requests against CSRF using antiforgery tokens.
- Validate inputs on the server.
- Use parameterized database access through EF Core.
- Do not expose account numbers or sensitive data unnecessarily.
- Never log passwords, full account numbers, or sensitive financial details.
- Use secure cookie settings appropriate to the deployment environment.
- Apply authorization checks to every operation that accesses financial records.
- Prevent unauthorized users from reading, editing, or deleting transactions.
- Create database indexes for frequently filtered columns.
- Use database transactions for operations involving multiple related writes.
- Enforce appropriate foreign-key constraints.

Provide a backup procedure for the SQLite database. Ensure backups are created consistently and can be restored. Do not copy an actively written SQLite database without using an appropriate backup mechanism.

Do not claim that the application is encrypted at rest unless database encryption is explicitly implemented and configured.

## 7. Performance Requirements

The application must remain lightweight.

- Use SQLite for local persistence.
- Avoid unnecessary database queries.
- Avoid N+1 query patterns.
- Use projections and aggregation for reporting.
- Use AsNoTracking for read-only queries where appropriate.
- Use pagination for large transaction lists.
- Load dashboard data efficiently.
- Avoid unnecessary JavaScript and CSS libraries.
- Serve static assets locally.
- Do not introduce caching unless measurement shows it is beneficial.
- Avoid unnecessary background services.
- Avoid premature optimization and overengineering.

Prefer simple code that can be understood and maintained by one developer.

## 8. Database and Financial Rules

Create EF Core entities, relationships, constraints, and migrations.

Use appropriate indexes for transaction dates, account IDs, categories, and transfer identifiers.

Define a single authoritative rule for calculating balances.

Do not maintain independently editable cached balances that can drift from the transaction ledger.

If performance later requires balance snapshots, implement them only with reconciliation tests and a reliable update strategy.

Ensure account opening balances are not counted as deposits during the reporting period.

A closing balance for a selected date range should be calculated using the opening balance plus all relevant ledger entries up to the end of the selected period.

Handle transaction edits and deletions so historical reports remain consistent.

Use explicit database transactions when multiple records must be updated atomically.

## 9. Testing Requirements

Write unit and integration tests for:

- Opening balance calculations.
- Deposits.
- Withdrawals.
- Transfers.
- Transfer rollback when either ledger entry fails.
- Date-range reports.
- Category aggregation.
- Multiple currencies.
- Invalid transaction amounts.
- Card-account consistency.
- Transaction editing and deletion.
- Balance reconciliation.
- Authentication and authorization.
- SQLite persistence and database constraints.

At minimum, verify that:

Opening balance = 1,000

Deposit = 500

Withdrawal = 200

Expected closing balance = 1,300

Also verify that an internal transfer changes the source and destination account balances without increasing total external income or expenses.

Do not declare the project complete if the solution does not build or the tests fail.

## 10. Development Workflow

Before writing code:

1. Inspect the existing repository and its files.
2. Identify the available .NET SDK and runtime.
3. Create or update `TODO.md`.
4. Define a minimal implementation plan.
5. Identify any existing conventions that must be preserved.

Then implement the project in small, verifiable phases.

Suggested phases:

**Phase 1 — Foundation**
- Solution and project structure.
- SQLite and EF Core configuration.
- Domain entities.
- Initial migration.
- Basic layout and navigation.

**Phase 2 — Accounts and Cards**
- Account CRUD.
- Card CRUD.
- Validation and database constraints.

**Phase 3 — Transactions**
- Deposits and withdrawals.
- Transaction history.
- Search, filtering, and pagination.
- Balance calculation.

**Phase 4 — Transfers and Categories**
- Atomic transfers.
- Linked ledger entries.
- Category management.

**Phase 5 — Dashboard and Reports**
- Summary cards.
- Monthly charts.
- Account and category reports.
- Reconciliation tests.

**Phase 6 — Security and Release**
- Authentication and authorization.
- Backup and restore procedure.
- Automated tests.
- Production configuration.
- Build and deployment documentation.

After each phase:

1. Run `dotnet build`.
2. Run relevant tests.
3. Fix all errors before proceeding.
4. Update `TODO.md`.
5. Document design decisions and remaining limitations.

Do not generate the entire project in one unverified step.

## 11. Deliverables

The final repository must include:

- Complete source code.
- Solution and project files.
- EF Core migrations.
- `TODO.md`.
- `README.md` with setup and run instructions.
- Database initialization instructions.
- Backup and restore instructions.
- Configuration examples.
- Automated tests.
- Deployment instructions.

The project must run with a small number of commands and without requiring cloud services.

Do not include fake production data or hardcoded report values.

## 12. Definition of Done

The application is complete when:

- The solution builds successfully.
- All required automated tests pass.
- Accounts and cards can be managed.
- Deposits and withdrawals can be recorded accurately.
- Account balances reconcile with the ledger.
- Transfers are atomic and do not distort income/expense reports.
- Reports and charts reflect real database data.
- Multiple currencies are handled without invalid aggregation.
- Financial data is protected by authentication and authorization.
- The application runs locally without an internet connection.
- The database can be backed up and restored.
- The codebase remains lightweight and maintainable.

**Final instruction:** Prioritize correctness, simplicity, and low resource consumption. If a feature adds substantial complexity without solving a stated requirement, do not implement it. Record it as a future enhancement in `TODO.md` instead.