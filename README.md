# Personal Finance Manager (مدیر مالی شخصی)

A lightweight, local-first personal finance manager built with **ASP.NET Core 9 MVC + SQLite**.
The UI is Persian (fa-IR) and right-to-left; all data stays on your machine, and the app works
fully offline after installation — no cloud services, no frontend build pipeline.

## Features

- **Accounts** — multi-currency bank accounts with opening balances, masked account numbers/IBANs, activate/deactivate.
- **Cards** — cards linked to accounts (last 4 digits only; full card numbers, PINs and CVVs are never stored).
- **Transactions** — deposits & withdrawals with categories, reference numbers, optional card link, server-side search / filter / sort / pagination.
- **Transfers** — atomic two-entry transfers between same-currency accounts, excluded from income/expense totals, with linked-entry navigation.
- **Dashboard & Reports** — per-currency totals, monthly deposit/withdrawal charts, expense distribution, account and category reports (Chart.js, all computed from real database records).
- **Security** — local cookie authentication (first-run setup, no default credentials), `PasswordHasher`, CSRF protection on every state-changing request, authorization on all endpoints.
- **Backups** — one-click consistent backup using SQLite's official online backup API, plus a documented restore procedure.

## Requirements

- .NET SDK **9.x** (`dotnet --list-sdks`)
- Any OS supported by .NET 9 (commands below are for Windows PowerShell)

> **Note (this machine):** the `dotnet` on PATH may resolve to a runtime-only copy.
> Use the full path `C:\Program Files\dotnet\dotnet.exe` (SDK 9.0.30x) for build/test/run.

## Quick start

```powershell
dotnet tool restore                                  # restores the local dotnet-ef tool
dotnet build FinanceManager.sln

cd src/FinanceManager.Web
dotnet run
```

Open <http://localhost:5212>. The first run shows a setup page — create your username and password.

- Data lives in `src/FinanceManager.Web/App_Data/` (`finance.db`, `backups/`, `keys/`).
- The database is created and migrated automatically at startup.

## Tests

```powershell
dotnet test
```

92 tests cover: money & currency rules (minor units, no silent rounding), the single ledger
balance rule (`opening + deposits − withdrawals`, including the 1000/500/200 → 1300 scenario),
transfers (atomicity & rollback, linked entries, exclusion from income/expense), date-range
reports, category aggregation, multi-currency isolation, invalid amounts, card–account
consistency, edit/delete consistency, balance reconciliation, authentication & authorization
(including full HTTP integration tests), and SQLite persistence and constraints.

## Migrations

Migrations live in `src/FinanceManager.Infrastructure/Migrations` and are applied automatically
at startup. To add one:

```powershell
dotnet ef migrations add NameOfMigration --project src/FinanceManager.Infrastructure --startup-project src/FinanceManager.Infrastructure
```

## Deployment (publish)

Framework-dependent:

```powershell
dotnet publish src/FinanceManager.Web -c Release -o publish
cd publish
./FinanceManager.Web.exe
```

Self-contained (no .NET runtime needed on the target machine):

```powershell
dotnet publish src/FinanceManager.Web -c Release -r win-x64 --self-contained true -o publish-win
```

### Configuration (`appsettings.json`)

| Key | Purpose |
| --- | --- |
| `App:DataDirectory` | Data folder (default `App_Data` relative to the app) |
| `ConnectionStrings:Default` | Optional; derived from `App:DataDirectory` when blank |
| `Kestrel:Endpoints:Http:Url` | Listen address (default `http://localhost:5212`) |

The default currency is a runtime setting (Settings page, stored in the database).

## Backup & restore

- **Backup:** Settings page → «ایجاد نسخه پشتیبان». Uses SQLite's official online backup API
  (safe while the app is running). Files land in `App_Data/backups` and can be downloaded from
  the same page.
- **Restore:** close the app → replace `App_Data/finance.db` with the backup file → delete
  `finance.db-wal` / `finance.db-shm` if present → start the app again.
- Never copy the live `finance.db` manually while the app is running.

## Architecture

```
FinanceManager.sln
├─ src/FinanceManager.Core            # entities, money/currency rules, Persian calendar & formatting (no EF dependency)
├─ src/FinanceManager.Infrastructure  # EF Core + SQLite, migrations, domain services, reporting, backups
├─ src/FinanceManager.Web             # MVC controllers, Razor views, cookie auth, local static assets
└─ tests/FinanceManager.Tests         # xUnit unit + integration tests
```

Key rules (see `docs/master-build-prompt.md` for the full spec):

- Amounts are stored as **integer minor units** (`long`); `decimal` is used only for conversion.
  Precision comes from a currency catalog (0/2/3 decimals); amounts exceeding the currency's
  precision are rejected — there is no silent rounding.
- Balances are always computed from the ledger: `opening + deposits − withdrawals`.
  No independently stored balance that could drift from the ledger.
- A transfer is one logical operation: two linked rows sharing a `TransferGroupId`, committed in
  a single database transaction, never counted as external income/expense.
- Different currencies are never added together; every report groups by currency.
- Dates are stored in the Gregorian calendar (`DateOnly`) and displayed with the Persian calendar.
  Reporting periods (this month / last month / this year) follow Persian calendar boundaries.

## Security notes

- All pages require authentication; the first-run setup page creates the initial account (no default credentials).
- Passwords are hashed with ASP.NET Core's `PasswordHasher` (PBKDF2 with salt).
- Antiforgery tokens are required on every state-changing request; cookies are `HttpOnly` +
  `SameSite=Lax` (`Secure` auto-negotiated on HTTPS).
- Account numbers/IBANs are masked in lists; store only card last-4 — full card numbers, PINs,
  and CVVs are never stored.
- The SQLite database is **not** encrypted at rest — protection relies on your OS account
  security. EF command logging stays at Warning level to avoid logging financial details.

## Current limitations / future ideas

- Single-user installation; password recovery is manual.
- No exchange rates / currency conversion (intentional in this phase).
- No PDF/Excel export yet.
- Persian dates are entered as text (e.g. `۱۴۰۳/۰۷/۲۲`) — no JavaScript calendar widget.
- Restore is a manual, documented procedure.
- Future: balance snapshots with reconciliation tests, Jalali date picker, report exports.
