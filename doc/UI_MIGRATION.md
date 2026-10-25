# UI migration — phases 0–3

Date: October 24, 2026. Branch: `feat/ui-design-migration`. Baseline: `6803c83d6a2aea89e684ca991fa67868fc593d59` (staging).

Implement the approved shared foundation, authentication and Borrower UI, then stop. Custodian and Admin page migrations and final whole-system acceptance (phases 4–6) require the user's next instruction. Shared components will affect their surrounding shell, but their workflow views are outside this batch.

## Design authority

Use the [complete role design on Figma Page 3](https://www.figma.com/design/F1NcWYPCQgjVKy0kUd5kI4/campus-equipment?node-id=578-5275), not the earlier 24-screen exploration. The source workspace has matching 36-screen exports under `doc/design-refresh/role-uniform` (uncommitted design work is deliberately preserved in the original checkout).

Retain the existing primary `#5980A6`, strong `#4A6F94`, tints `#D6EBFF`/`#EBF5FF`, canvas `#F2F2F3`, border `#D9D9DC`, text `#3A3C40`, muted `#6B6E73`, black and white. Use Barlow, rounded white cards, pill navigation, modular dashboard grids and responsive forms.

## Phase 0 — baseline and contract inventory

The baseline Release build/test run passed: **348 passed, 17 SQL Server opt-in tests skipped, 0 failed**, 365 total. Current implementations already include the catalog, reservations, calendar, approval/release, returns, profile management and reports. Older README status notes/design sample counts do not override the running code.

| Design / scope | Current views and behavior to retain |
| --- | --- |
| Shared shell | `_Layout`, `_Sidebar`, `_Topbar`, components; preserve role/permission predicates, demo role links, sign-out POST, mobile drawer IDs and script sections |
| S01 login | `Account/Login`; UserId, Password, RememberMe, returnUrl, POST, validation, password visibility and duplicate-submit handling |
| S02–S05 recovery | `ForgotPassword`, `ForgotPasswordCheckInbox`, `ResetPassword`; preserve six-digit OTP, resend POST, generic account-existence response, hidden email/user/token, expiry and validation |
| Supporting access | `Register`, `AccessDenied`; preserve all registration fields and eligibility message; preserve denial routing and sign-out behavior |
| B01 dashboard | `Dashboard/Borrower`; model-backed four summary counts, current loans, requests, rejection reasons, eligibility and late returns |
| B02 catalog | `Catalog/Index`, `Details`; Search/CategoryId/From/To, current images/metadata, booked windows, query-preserving pagination, permission-gated reserve link |
| B03/B05/B06 reservation | `Reservation/Create`; retain one server POST with EquipmentId, ReservationStart, ReservationEnd, Purpose, local-time format and server validation. Use visual sections rather than introducing a new multi-step workflow |
| Existing reservation screens | `Reservation/Index`, `Details`; retain status tabs, counts, page query, all timeline states, conditional cancellation and the existing confirmation dialog |
| B04 calendar | `AvailabilityCalendar/Index`; preserve FullCalendar day/week, events endpoint, filters, all data attributes/DOM IDs, permission gating, error handling and selection-to-reservation URL |
| B07/B08 notifications | Deferred: visual examples have no corresponding current notification workflow. Do not add simulated unread counts or inert notification actions |

### Scope boundaries

- Preserve controllers, services, DTO contracts, business rules, Identity/cookies, audit behavior, database entities and migrations. No production database migration is part of this change. Calendar JavaScript adds only two FullCalendar 7 presentation-class callbacks; its existing fetching, filtering, conflict checks and selection code remain unchanged.
- Preserve every existing `asp-for`, form action/method, route parameter, hidden value, antiforgery token and script hook. Model-backed data remains authoritative; do not copy Figma sample people/counts into application content.
- Adapt the Figma reset-link text to the existing OTP contract. Keep the current calendar engine and scheduling model, not a new resource-timeline backend.
- Keep existing interaction restrictions; existing unavailable dashboard actions remain unavailable.
- Do not mix in PR review reports or unrelated tracker changes from the original checkout.

## Verification and handoff

For this batch: build production frontend assets, build/test .NET, compare functional view contracts to baseline, and exercise the real pages at desktop/tablet/mobile sizes with a disposable local database. Check validation, role navigation, dropdowns, drawer keyboard behavior, catalog filters, reservation submission/details/cancellation, calendar events and denied access. Record actual results below; do not imply phases 4–6 are complete.

### Implementation results

Phases **0–3 are implemented**. Phases **4–6 have not been started** and await the user's instruction. Shared styling naturally appears around all roles; the Custodian/Admin workflow views have not been migrated.

- Added reusable rounded surfaces, controls, badges, summary cards, table/pager styles, authentication panels, decorative category equipment placeholders, and responsive shell styling using the existing palette. Actual equipment photos still take precedence over placeholders.
- Migrated the authentication layouts without changing their forms or scripts. Replaced the old decorative hard-coded 248/5/3 access-page statistics with the approved equipment illustration; live dashboard/model counts remain untouched.
- Migrated Borrower dashboard, catalog/details, reservation creation/list/details, cancellation dialog and the shared availability calendar presentation. Reservation equipment/schedule/purpose remain one existing server form, with visual sections instead of new server steps.
- Retained existing unavailable dashboard actions and disabled header equipment search; the existing catalog search remains the working search entry. No new notifications were introduced.
- Improved mobile drawer focus trapping and Escape focus restoration. Added a skip link and removed the duplicate header/catalog search DOM ID. Preserved navigation permission predicates and all hidden form values.
- FullCalendar 7 uses semantic roles and generated internal classes. Added supported `eventClass` / `eventInnerClass` callbacks for event appearance, styled the toolbar through semantic selectors, and retained the existing Reserved/Borrowed/Overdue labels and semantic colors.

### Checks actually completed

| Check | Result |
| --- | --- |
| Baseline Release build/test | 348 passed, 17 SQL Server opt-in tests skipped, 0 failed |
| Post-change Release build/test | 348 passed, 17 skipped, 0 failed; Razor views compiled successfully |
| Production frontend assets | `npm run assets:build` passed; CSS rebuilt after the final event-class styling |
| UI contracts | `node ASI.Basecode.WebApp/scripts/check-ui-contracts.mjs` passed for 15 views, hidden inputs and protected business/security code; calendar diff limited to the two appearance callbacks |
| JavaScript syntax | `node --check` passed for app-shell and availability-calendar scripts |
| Patch whitespace | `git diff --check` passed |
| Authentication browser checks | Sign-in/out, required-field feedback, password visibility, remember-me input, generic recovery confirmation for a nonexistent test address, invalid OTP feedback, registration required fields |
| Borrower browser checks | Model-backed populated/empty dashboards, catalog search and no-match state, equipment details, invalid-time validation retaining entered values, valid submission and stored detail view, status tabs, cancellation dismiss/confirm |
| Calendar browser checks | Real SQL-backed events, event colors, equipment filter, day/week switch, future-week navigation and dragging a free interval into the existing reservation form with correct equipment/from/to values |
| Role checks | Borrower-only account sees its permitted links; direct Users page request is denied and the return-to-dashboard link works |
| Responsive checks | 390-pixel authentication/forms and mobile submission; 768-pixel dashboard; 1440-pixel desktop dashboard/calendar. No page-level horizontal overflow observed in these checks; mobile drawer Shift+Tab wraps and Escape restores menu-button focus |

Browser checks used **only** the separate `Gearantee_UI_Migration_6803c83` LocalDB database, seeded with test records. Baseline login/dashboard visuals were inspected before replacing the preview views; after-change pages were inspected in the actual MVC application, not a static HTML mockup. The preview runs at `http://127.0.0.1:5186` from the ignored `artifacts/ui-preview` directory. Its configuration and fixture files are not part of the application patch.

### Verification limits

- The 17 opt-in SQL Server concurrency tests were not enabled; browser interactions did use a real isolated SQL Server database. Services, controllers and migrations are unchanged.
- Live email delivery and a successful emailed-OTP/password-change journey were not exercised. Existing OTP service tests passed; the UI retains the exact recovery fields, hidden values, actions and validation targets.
- The catalog sample fits on one page. Pager contracts are unchanged and covered by the contract comparison; a multi-page catalog was not manually exercised.
- Full-system acceptance and detailed Custodian/Admin workflow migration remain phases 4–6. Existing Google Sheet/WBS feature dates and completion statuses are unchanged because this is Additional work.

### Sidebar follow-up — October 24, 2026

At the user's request, removed the development-only role-switch panel. Navigation permissions, destinations and the sign-out form are unchanged. The sidebar now has a viewport-bounded card, compact short-desktop spacing and a fixed profile/sign-out footer. Only the navigation list scrolls when needed; its scrollbar is hidden while wheel, touch and keyboard access remain available. Short landscape screens use a smaller brand area.

Verified the running Visual Studio application with the multi-role demo account: all 11 navigation links, no role switcher or visible sidebar scrollbar, no sidebar overflow at 1366×768, mobile drawer at 390×844, and navigation scrolling with visible sign-out at 844×390. Checked keyboard access to the last link, drawer focus wrapping and Escape dismissal. The 15-view contract comparisons pass with the three intentionally removed demo links excluded; the script's subsequent whole-change guard still flags the separate pre-existing `Startup.Auth.cs` edit, which this follow-up preserves. The standalone preview's LocalDB startup failure prevents checking it there, so browser verification used the running Visual Studio app.

### Pagination follow-up — October 24, 2026

At the user's explicit request, standardized all nine paginated lists on 10 records: catalog, My Reservations, review queue, return desk, borrowing history, users, borrower profiles, equipment and categories. Eight services now use the shared `PaginationDefaults.PageSize`; this is an authorized page-size change beyond the original presentation-only batch. Filters, sorting, permissions, page clamping, calendar event fetching and exports are unchanged. Both equipment/category tabs retain their separate page parameter names and active-tab filters.

All lists share `Components/_Pagination` and `PaginationViewModel`: record ranges, a 10-per-page indicator, page count, bounded numbered links with ellipses, current-page/disabled states, and Previous/Next links. Mobile uses compact Previous/page count/Next controls. Table scroll wrappers now contain positioned screen-reader labels so they do not force horizontal page overflow.

Verification: Release build and the suite passed with 367 tests passed and 17 SQL Server opt-in tests skipped. An additional HTTP/Razor integration test then passed, navigating 23 filtered accounts over 10/10/3 rows through the rendered Next links and checking the final disabled state. Existing hostile-search encoding and category tab/filter pagination tests passed. Added coverage for all four return tabs, catalog and reservation boundaries, record ranges, large-page windows and preserved route parameters. Production CSS build and `git diff --check` passed.

Browser QA at 1366×900 and 390×844 used the actual Razor HTML saved by the SQLite integration test, served as a local, read-only fixture preview at port 5191. Checked Next/Previous navigation, first/current/last page states, 10/10/3 rendered rows and no mobile page overflow after the containment fix. These visual checks are not a claim of a new live SQL Server session. Rebuild and restart the Visual Studio app to load the new compiled page-size logic and pager model. The following handoff notes describe the updated staged contract checker for these authorized follow-ups; it continues to reject unrelated authentication changes.

### Git handoff — October 25, 2026

The handoff includes the UI migration and explicitly requested sidebar/header, Custodian seed and 10-record pagination follow-ups. Separate local lockout settings, login-label copy changes, earlier design exports/review reports and their four original tracker notes are excluded from the commit without modifying those working files.

The contract checker now supports `--staged`: it verifies unchanged form/hidden-field/script contracts, all nine pager filter/page parameters, exact page-size-only changes in the eight services, the shared 10-record constant and unchanged protected authentication/controller/persistence code. This mode checks the proposed commit independently of the separate local authentication edit. Build validation can substitute a copy of the unchanged baseline `Startup.Auth.cs` through an ignored MSBuild targets file so those local lockout changes stay untouched.

Handoff validation passed: full-solution Release build/test (368 passed, 17 SQL Server opt-in tests skipped), 22 view and nine pager contracts from the staged index, JavaScript syntax, production frontend asset build and generated-asset consistency, EF pending-model check (no changes), and NuGet vulnerability audit (none found from the configured sources). The Release check compiled the baseline authentication configuration through the validation targets; the working file remained unchanged. Remote CI status is separate from these local results.

### Continue later

Header follow-up: removed the disabled catalog-search placeholder from the shared topbar at the user's request. Verified the Borrower, Custodian and Administrator dashboards each render without a header search field. Working catalog filters and the Users page's account-search form remain unchanged.

Use `feat/ui-design-migration` directly in the original repository checkout. At the user's request, the UI changes were relocated there and the separate worktree was retired. Earlier uncommitted design files and PR review reports remain preserved locally, together with their separate tracker notes. A recovery ZIP is stored under the ignored `artifacts` directory. The scoped implementation is included in this handoff; unrelated design/review work remains local. Wait for the user before migrating the Custodian pages (phase 4), Admin pages (phase 5), or performing final whole-system acceptance (phase 6).
