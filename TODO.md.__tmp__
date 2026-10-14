# TODO — Personal Finance Manager (ASP.NET Core 9 · SQLite)

> Spec: `docs/master-build-prompt.md`
> Status: **COMPLETE** — all phases done · build green (Debug + Release) · **92/92 tests passing** · end-to-end smoke test passed
> Completed: 2026-10-14

## Environment (verified)

- OS: Windows 10.0.26200 (x64), shell: PowerShell
- .NET SDK: **9.0.303** at `C:\Program Files\dotnet`
  (PATH `dotnet` may resolve to a runtime-only copy — use the full path `C:\Program Files\dotnet\dotnet.exe`)
- ASP.NET Core runtime: 9.0.7 · EF Core packages pinned to 9.0.7 · local `dotnet-ef` tool via `.config/dotnet-tools.json`
- Static assets served locally (Bootstrap 5.3.3 RTL, Chart.js 4.4.7, Vazirmatn variable font, bootstrap-icons) — no CDN, no npm

## Phase checklist

### Phase 0 — Recon & planning
- [x] Inspect repository (empty except agent workspace files; single initial commit)
- [x] Identify .NET SDK / runtime
- [x] Copy master prompt to `docs/master-build-prompt.md`; create this file

### Phase 1 — Foundation
- [x] Solution + 4 projects (`Core`, `Infrastructure`, `Web`, `Tests`), references, `global.json`, local `dotnet-ef` tool
- [x] Local static assets downloaded and referenced (no internet needed at runtime)
- [x] Domain entities + enums (Account, Card, Category, Transaction, AppUser, AppSetting)
- [x] Money rules: currency catalog (0/2/3 decimals), exact minor-unit conversion, Persian number formatting, Persian (Jalali) date parse/format
- [x] `AppDbContext`: mappings, indexes, CHECK constraints, FK behaviors, default category seed
- [x] Initial migration `InitialCreate`
- [x] RTL layout + sidebar navigation + site CSS/JS (Vazirmatn, Persian number/date formatting everywhere)
- [x] `dotnet build` green

### Phase 2 — Accounts & cards
- [x] AccountService (CRUD, balance aggregation, activate/deactivate, guarded delete, currency-lock after transactions)
- [x] CardService (CRUD, card↔account consistency, guarded account change)
- [x] Accounts / Cards controllers + views with masking of account numbers/IBANs and Persian validation messages
- [x] build green

### Phase 3 — Transactions
- [x] TransactionService (create / edit / delete + all validation rules: positive amounts, active accounts, card↔account match, category-kind compatibility, transfer-entry protection)
- [x] Server-side search, filters, sorting, pagination (projections, `AsNoTracking`)
- [x] Transaction views (list / create / edit / details with transfer link)
- [x] build green

### Phase 4 — Transfers & categories
- [x] TransferService — two linked entries (`TransferGroupId`) in one database transaction; same-currency only; edit/delete as one logical operation; linked-entry navigation
- [x] CategoryService + default Persian category seed (editable/deactivatable)
- [x] Views + build green

### Phase 5 — Dashboard & reports
- [x] ReportService (per-currency aggregation; transfers excluded from external flows; Persian-month bucketing; day-level SQL + in-memory bucketing)
- [x] Dashboard: per-currency totals, period selector (this month / last month / this year / last 30 days / custom), monthly chart, category distribution, recent transactions, account table
- [x] Reports: summary, monthly, accounts, categories, transactions — with Chart.js, tables, empty states
- [x] Reconciliation verified by tests; build green

### Phase 6 — Security & release
- [x] Cookie auth (login / logout / first-run setup, no default credentials), `PasswordHasher`, global antiforgery, fallback authorization policy on everything, first-run lockdown middleware
- [x] Settings (default currency, change password), Backup via SQLite online backup API + download/delete + restore docs
- [x] Full xUnit suite (spec §9) — 92 tests, `dotnet test` green
- [x] README (setup / run / backup / deploy / config), `appsettings` samples, Release publish verified
- [x] End-to-end smoke test (setup → login → account → transaction → all pages → logout → bad/good login)

## Verification (Definition of Done)

- [x] Solution builds successfully (Debug and Release; `dotnet publish` produces runnable output)
- [x] All required automated tests pass — **92/92**
- [x] Accounts & cards manageable (CRUD + guards) — tests + smoke
- [x] Deposits/withdrawals recorded accurately — balance rule `opening + deposits − withdrawals` verified (1000/500/200 → 1300 scenario)
- [x] Balances reconcile with the ledger — reconciliation tests for summary/account reports
- [x] Transfers atomic and excluded from income/expense reports — including failure/rollback test at the storage layer
- [x] Reports and charts use real database data (no hardcoded values)
- [x] Multiple currencies handled without invalid aggregation — per-currency grouping tested
- [x] Financial data protected by authentication and authorization — incl. full HTTP integration tests
- [x] Runs locally without internet (all assets local; NuGet needed only to build)
- [x] Database backup & restore documented and automated (backup), manual (restore)
- [x] Codebase remains lightweight (no MediatR/AutoMapper/SPA/npm; thin controllers; single DbContext; ~4 projects)

## Design decisions log

- **Layering**: `Core` = entities + pure financial rules (money, currency, Persian text/calendar) with no EF dependency; `Infrastructure` = `AppDbContext`, migrations + application services (kept with the DbContext to avoid circular references — single-developer maintainability over ceremony); `Web` = MVC only.
- **Money**: stored as `INTEGER` minor units (`long`); `decimal` only for parsing/conversion; currency precision from catalog; excess decimals rejected (no silent rounding). Balance rule lives in one place (`LedgerMath`).
- **Calendar**: Gregorian `DateOnly` storage; Persian calendar for display and for period boundaries (this month/year = Jalali months/years).
- **Auth**: single-user local cookie auth; first-run setup creates the account; fallback authorization policy makes everything private by default; first-run middleware redirects everyone to setup until a user exists.
- **Transfers**: two linked rows, single DB transaction; excluded from external flows by `TransferGroupId IS NULL` filters in every aggregate.
- **Persian output**: `WebEncoderOptions` configured with `UnicodeRanges.All` so Persian text renders directly instead of numeric entities (HTML-dangerous characters are still encoded).
- **Config reads deferred to DI resolution** (paths/connection string/DP keys) so configuration overrides — including integration-test overrides — always apply.
- **Services return `ServiceResult` with error codes** (testable) + Persian messages (displayed via ModelState).
- **Seed data**: 8 default Persian categories only (editable); no fake financial data anywhere.

## Known limitations / future enhancements

- Single-user installation; no multi-user roles.
- No exchange rates / currency conversion — cross-currency aggregation is intentionally blocked.
- No PDF/Excel export (spec: later phase).
- Persian dates entered as text (`۱۴۰۳/۰۷/۲۲`); no JS Jalali date-picker widget yet.
- Restore is manual (documented) — no in-app restore to avoid replacing the live database unexpectedly.
- No balance snapshots yet; if performance ever requires them, they must come with reconciliation tests (spec §8).
- No HTTPS by default for the local deployment (HTTP on localhost); use a reverse proxy or `--urls https://...` if exposing the app.
