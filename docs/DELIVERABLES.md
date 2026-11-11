# BookWise: Deliverables Breakdown

This file splits every phase of `bookwise-execution-plan.md` into **small, individually shippable deliverables**. Each deliverable has:

- **What:** the thing you produce
- **Done when:** objective acceptance criteria (if you cannot tick them, it is not done)
- **Proof:** the evidence that exists in the repo (a test, a file, a screenshot, a URL)
- **Effort:** a rough estimate in hours

Deliverables marked **[KEY]** are the ones that carry the most hiring value. If time runs short, protect these first.

**ID format:** `D<phase>.<number>`, for example `D4.6` is deliverable 6 of Phase 4. Use the ID in your commit messages and pull request titles (`D4.6: add overlap exclusion constraint`).

---

## Summary Index

| Phase | Deliverables | Hours | Milestone at the end |
|---|---|---|---|
| 0. Environment and repository | D0.1 to D0.3 | 5 | Machine ready, repo exists |
| 1. Foundation | D1.1 to D1.6 | 12 | API runs, database connected, CI green |
| 2. Tenancy and authentication | D2.1 to D2.6 | 22 | Register, log in, isolation proven |
| 3. Core domain | D3.1 to D3.6 | 20 | A business can be fully configured |
| 4. Booking engine | D4.1 to D4.8 | 30 | Correct bookings, no double-booking |
| 5. Background jobs and email | D5.1 to D5.6 | 16 | Confirmations and reminders work |
| 6. Hardening | D6.1 to D6.6 | 16 | Policies, throttling, audit trail |
| 7. Testing and CI | D7.1 to D7.4 | 14 | Coverage visible, pipeline green |
| 8. Frontend | D8.1 to D8.6 | 34 | Full booking journey in the browser |
| 9. Deployment and documentation | D9.1 to D9.6 | 18 | Live demo and polished README |
| **Total** | **53 deliverables** | **~187 hours** | About 14 weeks at roughly 13 hours per week |

Estimates assume you are new to .NET. Add 20 percent buffer for the first two phases while you learn the tooling.

### Minimum viable portfolio cut line

If you must stop early, this is the smallest set that still makes a strong resume project:
**D1.1 to D1.5, D2.1 to D2.6, D3.1 to D3.5, D4.1 to D4.8, D7.2, D9.2, D9.5.**
That gives you a deployed, tenant-isolated, concurrency-safe booking API with the proof tests and a good README.

---

## Phase 0: Environment and Repository (5 h)

### D0.1: Development environment installed (3 h)
- **What:** all required tools installed and working.
- **Done when:**
  - [ ] `dotnet --version` prints 10.x
  - [ ] `git --version` prints a version, and `user.name` and `user.email` are set
  - [ ] `docker run hello-world` succeeds
  - [ ] An editor (Visual Studio Community, or VS Code with C# Dev Kit) opens a C# project with IntelliSense working
  - [ ] `dotnet ef` prints its help text
  - [ ] DBeaver or pgAdmin is installed
- **Proof:** a short `docs/SETUP.md` listing the versions you installed.

### D0.2: Repository created (1 h)
- **What:** a GitHub repository with sensible defaults.
- **Done when:**
  - [ ] Repo `bookwise` exists and is cloned locally
  - [ ] `.gitignore` (VisualStudio template) is committed
  - [ ] `README.md` has a one-line description
  - [ ] `docs/PLAN.md` and `docs/DELIVERABLES.md` (these two files) are committed
- **Proof:** the GitHub repo URL.

### D0.3: Smoke test passed (1 h)
- **What:** confirmation that the whole toolchain works end to end.
- **Done when:**
  - [ ] A throwaway `dotnet new webapi` project runs and responds in the browser
  - [ ] A PostgreSQL container starts and you connect to it from DBeaver
  - [ ] The throwaway project is deleted (not committed)
- **Proof:** a screenshot of the running API and the database connection (kept locally, not required in the repo).

---

## Phase 1: Foundation (12 h)

### D1.1: Layered solution skeleton (2 h)
- **What:** the solution with Domain, Application, Infrastructure, Api, and two test projects, with correct references.
- **Done when:**
  - [ ] `dotnet build` succeeds with zero warnings (`TreatWarningsAsErrors` on)
  - [ ] Domain has **no** project references
  - [ ] Application references only Domain; Infrastructure references Application; Api references Application and Infrastructure
  - [ ] `Directory.Build.props` sets `Nullable` and warning policy once for all projects
  - [ ] Template boilerplate deleted
- **Proof:** the folder tree in the repo and a green local build.

### D1.2: Local services with Docker Compose (1 h)
- **What:** PostgreSQL and Mailpit start with one command.
- **Done when:**
  - [ ] `docker compose up -d` starts both containers
  - [ ] PostgreSQL data survives `docker compose down` (named volume)
  - [ ] Mailpit UI opens at `http://localhost:8025`
- **Proof:** `docker-compose.yml` committed.

### D1.3: Database connection and initial migration (3 h)
- **What:** EF Core connected to PostgreSQL with the first migration applied.
- **Done when:**
  - [ ] `AppDbContext` exists in Infrastructure and is registered in the API
  - [ ] Connection string comes from configuration; no passwords committed (User Secrets or env vars for anything real)
  - [ ] The initial migration enables the `btree_gist` extension
  - [ ] `dotnet ef database update` creates the schema
  - [ ] A `__EFMigrationsHistory` table is visible in DBeaver
- **Proof:** `Migrations/` folder committed.

### D1.4: Cross-cutting API basics (4 h)
- **What:** the plumbing every later feature relies on.
- **Done when:**
  - [ ] Serilog writes structured logs to the console, with request logging
  - [ ] An unhandled exception returns a `ProblemDetails` JSON response (no stack trace leaks)
  - [ ] `/health` returns healthy only when the database is reachable
  - [ ] Swagger/OpenAPI UI loads in Development
  - [ ] FluentValidation is wired so invalid requests return `400` with a field-error list
- **Proof:** a sample invalid request and its `400` response pasted into `docs/DECISIONS.md`.

### D1.5: CI pipeline v1 (1 h)
- **What:** automatic build and test on every push.
- **Done when:**
  - [ ] `.github/workflows/ci.yml` restores, builds, and runs tests
  - [ ] A pull request shows the check passing
  - [ ] A build badge is in the README
- **Proof:** a green workflow run.

### D1.6: Run-locally instructions v0 (1 h)
- **What:** a README section that lets a stranger start the project.
- **Done when:**
  - [ ] A fresh clone, following only the README, reaches a working `/health` response
- **Proof:** you tested it on a clean clone (or a second folder).

---

## Phase 2: Tenancy and Authentication (22 h)

### D2.1: Tenant and user schema (3 h)
- **What:** `Tenant`, `User`, and `RefreshToken` entities with migrations.
- **Done when:**
  - [ ] `Tenant` has `Id`, `Name`, `Slug` (unique), `TimeZone`
  - [ ] `User` has `TenantId`, unique email per tenant, hashed password, role
  - [ ] `ITenantOwned` interface exists (`TenantId`)
  - [ ] Migration applies cleanly
- **Proof:** the migration file and the tables visible in DBeaver.

### D2.2: Tenant context and global query filters [KEY] (5 h)
- **What:** automatic tenant scoping of every query and write.
- **Done when:**
  - [ ] `ITenantContext` (scoped) exposes the current `TenantId`
  - [ ] Middleware fills it from the JWT claim (or the tenant slug on public routes)
  - [ ] Every `ITenantOwned` entity has a global query filter
  - [ ] `SaveChangesAsync` stamps `TenantId` on new rows and rejects modification of another tenant's rows
  - [ ] There is a deliberate, documented way to bypass the filter (`IgnoreQueryFilters`) used only in a few reviewed places
- **Proof:** unit tests for the stamping logic, plus D2.6.

### D2.3: Tenant registration endpoint (2 h)
- **What:** `POST /api/auth/register-tenant`.
- **Done when:**
  - [ ] Creates Tenant and Owner user in a **single transaction** (both or neither)
  - [ ] Duplicate slug returns `409`
  - [ ] Weak password or invalid time zone returns `400`
  - [ ] Passwords are hashed, never stored or logged in plain text
- **Proof:** Swagger screenshot and an integration test.

### D2.4: Login with JWT access tokens (3 h)
- **What:** `POST /api/auth/login`.
- **Done when:**
  - [ ] Correct credentials return a short-lived access token (about 15 minutes)
  - [ ] The token includes `sub`, `tenant_id`, and `role` claims
  - [ ] Wrong credentials return `401` with a generic message (no "user not found" vs "wrong password" difference)
  - [ ] A protected test endpoint returns `401` without a token and `200` with one
  - [ ] Signing key comes from configuration, not source code
- **Proof:** integration tests for both outcomes.

### D2.5: Refresh-token rotation and logout [KEY] (4 h)
- **What:** `POST /api/auth/refresh` and `POST /api/auth/logout`.
- **Done when:**
  - [ ] Refresh tokens are random, stored **hashed**, with an expiry
  - [ ] Using a refresh token issues a new pair and **revokes** the old token
  - [ ] Reusing a revoked token is rejected, and the whole token family for that user is revoked (reuse detection)
  - [ ] Logout revokes the current refresh token
  - [ ] Expired tokens are rejected
- **Proof:** integration tests covering rotation, reuse, expiry, and logout.

### D2.6: Tenant isolation test [KEY] (5 h)
- **What:** the test that proves tenancy works.
- **Done when:**
  - [ ] Test infrastructure exists: `WebApplicationFactory` plus Testcontainers PostgreSQL, shared across tests
  - [ ] The test creates tenants A and B, each with data
  - [ ] A user from A **cannot** list, fetch by id, update, or delete B's records (all return empty or `404`)
  - [ ] The test runs in CI
- **Proof:** the test name appears in a green CI run.

---

## Phase 3: Core Domain (20 h)

### D3.1: Service catalog (4 h)
- **What:** CRUD for services (name, duration, price, buffer).
- **Done when:**
  - [ ] Owner can create, list, update, and deactivate services
  - [ ] Duration must be greater than 0 and a multiple of the slot size you choose (for example 15 minutes)
  - [ ] Deactivated services stay in history but cannot be booked
  - [ ] Non-owners get `403`
- **Proof:** endpoint tests and OpenAPI documentation.

### D3.2: Staff management and invitations (5 h)
- **What:** owners add staff and staff can sign in.
- **Done when:**
  - [ ] Owner creates a staff profile and an invitation (token or temporary password)
  - [ ] Staff completes the invitation and logs in with the `Staff` role
  - [ ] Owner can deactivate staff; a deactivated staff member cannot log in or be booked
  - [ ] Invitation tokens expire and are single-use
- **Proof:** integration test of the full invite-to-login flow.

### D3.3: Working hours (4 h)
- **What:** `PUT /api/staff/{id}/working-hours` (replace the weekly schedule).
- **Done when:**
  - [ ] Multiple blocks per day are allowed (for example 09:00 to 13:00 and 14:00 to 18:00)
  - [ ] Overlapping or reversed blocks are rejected with a clear message
  - [ ] Times are stored as local wall-clock times plus the tenant time zone
  - [ ] Staff can edit only their own hours unless the owner edits them
- **Proof:** unit tests for the validation rules.

### D3.4: Time off (2 h)
- **What:** blocked periods for staff (leave, holidays).
- **Done when:**
  - [ ] Create, list, and delete time-off periods
  - [ ] End must be after start
  - [ ] Creating time off that conflicts with existing bookings returns a warning or `409` (choose and document the rule)
- **Proof:** tests for both rules.

### D3.5: Staff and service assignment (2 h)
- **What:** which staff member can perform which service.
- **Done when:**
  - [ ] `PUT /api/staff/{id}/services` replaces the assignment set
  - [ ] Assigning a service from another tenant is impossible (covered by a test)
- **Proof:** an integration test.

### D3.6: Reusable pagination, sorting, and filtering helper (3 h)
- **What:** a single shared way to page lists.
- **Done when:**
  - [ ] A `PagedResult<T>` and query-parameter model exist
  - [ ] `pageSize` has a maximum (for example 100)
  - [ ] Sorting accepts only whitelisted fields (no arbitrary column names from the client)
  - [ ] All list endpoints from D3.1 to D3.4 use it
- **Proof:** unit tests for edge cases (page 0, huge page size, unknown sort field).

---

## Phase 4: Booking Engine (30 h)

### D4.1: Availability calculator, pure domain logic [KEY] (8 h)
- **What:** a function that turns schedules and bookings into free slots, with no database access.
- **Done when:**
  - [ ] Inputs: working hours, time off, existing bookings, service duration, buffer, tenant time zone, target date
  - [ ] Output: list of free start times in UTC
  - [ ] Handles: back-to-back bookings, buffers, time off mid-day, past times, day boundaries, and daylight-saving transitions (test with a zone that has DST, even if Pakistan currently does not)
  - [ ] At least **20 unit tests**, including edge cases
- **Proof:** the unit test project, all green.

### D4.2: Availability endpoint (1 h)
- **What:** `GET /api/availability?serviceId=&staffId=&date=`.
- **Done when:**
  - [ ] Returns slots for one staff member
  - [ ] "Any staff" mode (omit `staffId`) returns merged slots, noting which staff are free
  - [ ] Respects the service-to-staff assignment from D3.5
- **Proof:** integration test with seeded schedules.

### D4.3: Booking entity and status machine (3 h)
- **What:** the `Booking` aggregate with enforced transitions.
- **Done when:**
  - [ ] Statuses: `Pending`, `Confirmed`, `Completed`, `Cancelled`, `NoShow`
  - [ ] Illegal transitions throw a domain exception (for example `Completed` to `Pending`)
  - [ ] Transition rules live in the Domain project, not in controllers
- **Proof:** unit tests covering every allowed and disallowed transition.

### D4.4: Customer accounts (3 h)
- **What:** customers who belong to a tenant.
- **Done when:**
  - [ ] Customer can register and log in under a specific tenant slug
  - [ ] The same email can exist in two tenants as two separate customers
  - [ ] Customers can only see their own bookings
- **Proof:** integration tests.

### D4.5: Create booking endpoint (4 h)
- **What:** `POST /api/bookings`.
- **Done when:**
  - [ ] Validates that the slot is inside working hours and not in the past
  - [ ] Validates staff-service assignment and that the service is active
  - [ ] Computes the end time from the service duration (never trusts a client-supplied end time)
  - [ ] Returns `201` with the booking details
- **Proof:** integration tests for success and each validation failure.

### D4.6: Overlap exclusion constraint and conflict handling [KEY] (3 h)
- **What:** the database guarantees no overlapping bookings.
- **Done when:**
  - [ ] A migration adds the `EXCLUDE USING gist` constraint on `(staff_id, tstzrange(start_time, end_time))` ignoring cancelled bookings
  - [ ] A violation (SQLSTATE `23P01`) is caught and mapped to `409 Conflict` with a friendly message
  - [ ] Cancelling a booking frees the slot immediately
- **Proof:** a migration test or SQL check, plus D4.8.

### D4.7: Cancel and reschedule (4 h)
- **What:** `POST /api/bookings/{id}/cancel` and `/reschedule`.
- **Done when:**
  - [ ] Cancel moves status to `Cancelled` and records history
  - [ ] Reschedule runs in **one transaction**: the old slot is released only if the new slot is secured
  - [ ] Rescheduling into an occupied slot returns `409` and leaves the original booking untouched
  - [ ] Only permitted users can act (basic rules now, full policies in D6.1)
- **Proof:** integration tests, including the failed-reschedule case.

### D4.8: Concurrency test [KEY] (4 h)
- **What:** proof that double-booking is impossible under load.
- **Done when:**
  - [ ] A test sends **20 simultaneous** booking requests for the same slot using separate customers
  - [ ] Exactly **one** succeeds with `201` and **nineteen** return `409`
  - [ ] The database contains exactly one active booking for that slot afterward
  - [ ] The test runs in CI and is stable (run it 10 times locally with no flakes)
- **Proof:** the test name in CI. This is the single best thing to mention in interviews.

---

## Phase 5: Background Jobs and Email (16 h)

### D5.1: Hangfire installed and dashboard secured (2 h)
- **What:** a job system backed by PostgreSQL.
- **Done when:**
  - [ ] Hangfire uses PostgreSQL storage
  - [ ] The `/hangfire` dashboard requires an authenticated Owner or admin; anonymous access is impossible
  - [ ] A trivial test job runs and shows in the dashboard
- **Proof:** a dashboard screenshot and a test that anonymous access returns `401` or `403`.

### D5.2: Email abstraction and MailKit sender (3 h)
- **What:** `IEmailSender` with a real SMTP implementation.
- **Done when:**
  - [ ] Interface lives in Application, implementation in Infrastructure
  - [ ] SMTP settings come from configuration
  - [ ] In development, mail arrives in Mailpit
  - [ ] A fake sender is used in tests
- **Proof:** an email visible in Mailpit.

### D5.3: Email templates (2 h)
- **What:** confirmation, reminder, and cancellation emails.
- **Done when:**
  - [ ] Each email shows business name, service, staff, local date and time (in the tenant's time zone), and a cancel link
  - [ ] HTML renders correctly in the Mailpit preview
  - [ ] User-supplied text is HTML-encoded
- **Proof:** screenshots of the three emails.

### D5.4: Confirmation email on booking (1 h)
- **What:** an email sent right after a booking.
- **Done when:**
  - [ ] The API responds without waiting for the email to send (fire-and-forget job)
  - [ ] A failing SMTP server does not fail the booking request
- **Proof:** a test with a fake sender that throws.

### D5.5: Reminder scheduling and cancellation [KEY] (4 h)
- **What:** a reminder 24 hours before each appointment.
- **Done when:**
  - [ ] Creating a booking schedules a delayed job; the Hangfire job id is stored on the booking
  - [ ] Cancelling deletes the scheduled job
  - [ ] Rescheduling deletes the old job and schedules a new one
  - [ ] Bookings made less than 24 hours ahead skip the reminder (or send an immediate one; document the rule)
- **Proof:** an integration test asserting the job is removed on cancel.

### D5.6: Idempotent, tenant-aware jobs and expiry job (4 h)
- **What:** jobs that are safe to retry.
- **Done when:**
  - [ ] Jobs carry `TenantId` and set the tenant context before touching data
  - [ ] A reminder runs twice and sends only **one** email (`ReminderSentAt` guard)
  - [ ] Jobs skip cancelled bookings
  - [ ] A recurring job expires `Pending` bookings older than 15 minutes
  - [ ] Retries use backoff, and failures are logged with tenant and booking ids
- **Proof:** tests for the double-run case and the cancelled-booking case.

---

## Phase 6: Hardening (16 h)

### D6.1: Policy-based authorization [KEY] (5 h)
- **What:** rules beyond simple roles.
- **Done when:**
  - [ ] Policies exist: `CanManageStaff`, `CanViewBooking`, `CanCancelBooking`
  - [ ] `CanViewBooking` allows the owner, the assigned staff member, or the customer who owns it, and nobody else
  - [ ] At least one policy uses a custom `IAuthorizationRequirement` and handler with resource-based checks
  - [ ] Controllers or endpoints reference policy names, not hard-coded role strings
- **Proof:** authorization tests (allowed and forbidden cases for each role).

### D6.2: Tenant settings enforced (3 h)
- **What:** business rules each tenant can configure.
- **Done when:**
  - [ ] Settings: cancellation window (hours), minimum lead time, maximum days ahead
  - [ ] The booking and cancel flows enforce them in the domain
  - [ ] Owners can read and update settings
- **Proof:** tests for each limit (just inside and just outside).

### D6.3: Rate limiting (1 h)
- **What:** protection against abuse.
- **Done when:**
  - [ ] Login, tenant registration, and booking creation are rate limited
  - [ ] Exceeding the limit returns `429` with a `Retry-After` header
  - [ ] Limits are configurable
- **Proof:** an integration test that triggers `429`.

### D6.4: Booking audit trail (2 h)
- **What:** history of every status change.
- **Done when:**
  - [ ] Each transition writes a `BookingHistory` row (old status, new status, who, when)
  - [ ] `GET /api/bookings/{id}/history` returns it to authorized users
- **Proof:** a test that checks history after create, cancel, and reschedule.

### D6.5: Indexes and query review (3 h)
- **What:** acceptable performance on realistic data.
- **Done when:**
  - [ ] Composite indexes start with `TenantId` on bookings, staff, and services
  - [ ] A seed script generates around 100,000 bookings across 50 tenants
  - [ ] The availability and booking-list queries are checked with `EXPLAIN ANALYZE` and use indexes
  - [ ] Findings are written in `docs/DECISIONS.md`
- **Proof:** the `EXPLAIN` output saved in the docs.

### D6.6: Security checklist and transport hardening (2 h)
- **What:** a documented security posture.
- **Done when:**
  - [ ] CORS allows only the frontend origin
  - [ ] HTTPS redirection and security headers are on
  - [ ] No secrets in Git (verified by searching history)
  - [ ] `docs/SECURITY.md` lists each protection and the test or code that proves it
- **Proof:** the `SECURITY.md` file.

---

## Phase 7: Testing and CI (14 h)

### D7.1: Unit test suite (5 h)
- **What:** fast tests for domain and application logic.
- **Done when:**
  - [ ] Availability, status machine, policy handlers, and settings rules are covered
  - [ ] The suite runs in under 10 seconds with no Docker needed
- **Proof:** a green `dotnet test` for the unit project.

### D7.2: Integration test suite [KEY] (5 h)
- **What:** end-to-end tests against a real PostgreSQL.
- **Done when:**
  - [ ] Includes: tenant isolation, concurrent booking, refresh rotation and reuse, reminder removal on cancel, authorization failures, and rate limiting
  - [ ] One shared container per test run (not one per test) to keep it fast
  - [ ] Tests are independent and can run in any order
- **Proof:** a green CI run listing these tests.

### D7.3: Coverage reporting (2 h)
- **What:** visible quality metrics.
- **Done when:**
  - [ ] Coverlet generates a report in CI
  - [ ] Domain and Application layers are at 70 to 80 percent or higher
  - [ ] A coverage badge is in the README
- **Proof:** the badge.

### D7.4: CI pipeline v2 (2 h)
- **What:** a complete pipeline.
- **Done when:**
  - [ ] Build, unit tests, integration tests, coverage, and a Docker image build all run
  - [ ] Dependencies are cached to keep it fast (under about 5 minutes)
  - [ ] Pull requests cannot merge with a failing pipeline (branch protection on `main`)
- **Proof:** a green pipeline and the branch-protection setting.

---

## Phase 8: Frontend (34 h)

### D8.1: Frontend scaffold and typed API client (3 h)
- **What:** the Next.js project and a generated client.
- **Done when:**
  - [ ] Next.js, TypeScript, Tailwind, TanStack Query, and React Hook Form + Zod are installed
  - [ ] Types are generated from the OpenAPI document (for example with `openapi-typescript`)
  - [ ] An environment variable controls the API base URL
- **Proof:** the app runs and calls `/health`.

### D8.2: Authentication screens (4 h)
- **What:** register business, log in, log out.
- **Done when:**
  - [ ] Forms validate client-side and show server errors
  - [ ] Access token refresh happens automatically on `401`
  - [ ] Protected pages redirect when signed out
- **Proof:** a working login to dashboard flow.

### D8.3: Customer booking flow [KEY] (10 h)
- **What:** the public page where customers book.
- **Done when:**
  - [ ] URL pattern per tenant (for example `/b/{slug}`)
  - [ ] Steps: choose service, choose staff (or "any"), choose date and time slot, enter details, confirm
  - [ ] Slots display in the **business's** time zone, with the zone shown
  - [ ] A `409` ("slot just taken") refreshes the slot list with a clear message
  - [ ] A confirmation page appears, and the confirmation email arrives in Mailpit
- **Proof:** a 60 to 90 second screen recording.

### D8.4: Owner dashboard (9 h)
- **What:** the management interface.
- **Done when:**
  - [ ] Manage services and staff
  - [ ] Working-hours editor and time-off management
  - [ ] Bookings list with filters, pagination, cancel, and reschedule
  - [ ] Tenant settings page
- **Proof:** screenshots of each screen.

### D8.5: Staff calendar (4 h)
- **What:** a calendar view of a staff member's own bookings.
- **Done when:**
  - [ ] Day and week views using FullCalendar
  - [ ] Clicking a booking shows details and allowed actions
  - [ ] Staff see only their own bookings
- **Proof:** a screenshot and a manual test with two staff accounts.

### D8.6: Polish: states, errors, responsiveness (4 h)
- **What:** the finishing work that makes it feel real.
- **Done when:**
  - [ ] Every list has loading, empty, and error states
  - [ ] Every form shows pending state and prevents double submit
  - [ ] Layout works at phone width (about 375 px) and desktop
  - [ ] Basic keyboard navigation and form labels work
- **Proof:** screenshots at both widths.

---

## Phase 9: Deployment and Documentation (18 h)

### D9.1: Production Dockerfile (2 h)
- **What:** a lean, secure image for the API.
- **Done when:**
  - [ ] Multi-stage build; final image uses the ASP.NET runtime image
  - [ ] Runs as a non-root user
  - [ ] Health check is defined
- **Proof:** `docker build` and `docker run` work locally.

### D9.2: Deployed API, database, and frontend [KEY] (5 h)
- **What:** everything live on the internet.
- **Done when:**
  - [ ] API and PostgreSQL run on a host (Azure, Railway, Render, or Fly.io); frontend runs on Vercel
  - [ ] Secrets are environment variables; migrations apply on release
  - [ ] CORS and cookies work between the real frontend and API domains
  - [ ] The public `/health` returns healthy
- **Proof:** the live URLs.

### D9.3: Demo tenant seed (2 h)
- **What:** a ready-to-use demo for recruiters.
- **Done when:**
  - [ ] A demo business has staff, services, hours, and sample bookings
  - [ ] Demo credentials for owner and customer are in the README
  - [ ] The demo data can be reset by a script
  - [ ] The demo email sender is safe (uses a sandbox or logs rather than emailing strangers)
- **Proof:** log in with the documented credentials on the live site.

### D9.4: Continuous deployment (2 h)
- **What:** deploy automatically from `main` after tests pass.
- **Done when:**
  - [ ] A merge to `main` triggers deployment only after the pipeline is green
  - [ ] A failed deployment does not take the site down (the previous version keeps running)
- **Proof:** a deployment log tied to a commit.

### D9.5: README, architecture diagram, and decisions document [KEY] (5 h)
- **What:** the documents recruiters actually read.
- **Done when:**
  - [ ] README contains: one-paragraph pitch, GIF or screenshot, live link, demo credentials, tech stack, architecture diagram, how to run locally, how to run tests
  - [ ] Dedicated sections explain tenant isolation and double-booking prevention (with the SQL constraint)
  - [ ] `docs/DECISIONS.md` has at least 6 entries, each stating context, options, choice, and trade-off
  - [ ] Known limitations and future work are listed honestly
  - [ ] A stranger can understand the project in 3 minutes
- **Proof:** ask a friend to read it and tell you what the project does.

### D9.6: Demo recording and profile updates (2 h)
- **What:** making the project visible.
- **Done when:**
  - [ ] A 60 to 90 second screen recording is embedded in the README
  - [ ] The repository is pinned on your GitHub profile
  - [ ] Resume and LinkedIn have the project entry with real, verified numbers (test count, coverage, live link)
- **Proof:** the updated resume and profile.

---

## Deliverable Tracker

Copy this table into your repo and update it as you go.

| ID | Deliverable | Status | Date done | PR |
|---|---|---|---|---|
| D0.1 | Environment installed | Done | 2026-11-11 | |
| D0.2 | Repository created | Done | 2026-11-11 | |
| D0.3 | Smoke test passed | Done | 2026-11-11 | |
| D1.1 | Layered solution skeleton | Done | 2026-11-11 | |
| D1.2 | Docker Compose services | Done | 2026-11-11 | |
| D1.3 | Database and initial migration | Done | 2026-11-11 | |
| D1.4 | Cross-cutting API basics | Done | 2026-11-11 | |
| D1.5 | CI pipeline v1 | Done | 2026-11-11 | |
| D1.6 | Run-locally instructions | Done | 2026-11-11 | |
| D2.1 | Tenant and user schema | Not started | | |
| D2.2 | Tenant context and query filters [KEY] | Not started | | |
| D2.3 | Tenant registration | Not started | | |
| D2.4 | Login with JWT | Not started | | |
| D2.5 | Refresh rotation and logout [KEY] | Not started | | |
| D2.6 | Tenant isolation test [KEY] | Not started | | |
| D3.1 | Service catalog | Not started | | |
| D3.2 | Staff management and invites | Not started | | |
| D3.3 | Working hours | Not started | | |
| D3.4 | Time off | Not started | | |
| D3.5 | Staff-service assignment | Not started | | |
| D3.6 | Pagination helper | Not started | | |
| D4.1 | Availability calculator [KEY] | Not started | | |
| D4.2 | Availability endpoint | Not started | | |
| D4.3 | Booking status machine | Not started | | |
| D4.4 | Customer accounts | Not started | | |
| D4.5 | Create booking | Not started | | |
| D4.6 | Overlap constraint [KEY] | Not started | | |
| D4.7 | Cancel and reschedule | Not started | | |
| D4.8 | Concurrency test [KEY] | Not started | | |
| D5.1 | Hangfire and secured dashboard | Not started | | |
| D5.2 | Email abstraction and sender | Not started | | |
| D5.3 | Email templates | Not started | | |
| D5.4 | Confirmation email | Not started | | |
| D5.5 | Reminder scheduling [KEY] | Not started | | |
| D5.6 | Idempotent jobs and expiry | Not started | | |
| D6.1 | Policy-based authorization [KEY] | Not started | | |
| D6.2 | Tenant settings enforced | Not started | | |
| D6.3 | Rate limiting | Not started | | |
| D6.4 | Audit trail | Not started | | |
| D6.5 | Indexes and query review | Not started | | |
| D6.6 | Security checklist | Not started | | |
| D7.1 | Unit test suite | Not started | | |
| D7.2 | Integration test suite [KEY] | Not started | | |
| D7.3 | Coverage reporting | Not started | | |
| D7.4 | CI pipeline v2 | Not started | | |
| D8.1 | Frontend scaffold and client | Not started | | |
| D8.2 | Authentication screens | Not started | | |
| D8.3 | Customer booking flow [KEY] | Not started | | |
| D8.4 | Owner dashboard | Not started | | |
| D8.5 | Staff calendar | Not started | | |
| D8.6 | Polish and responsiveness | Not started | | |
| D9.1 | Production Dockerfile | Not started | | |
| D9.2 | Deployed system [KEY] | Not started | | |
| D9.3 | Demo tenant seed | Not started | | |
| D9.4 | Continuous deployment | Not started | | |
| D9.5 | README and decisions [KEY] | Not started | | |
| D9.6 | Demo recording and profiles | Not started | | |

---

## Suggested Weekly Targets

| Week | Deliverables |
|---|---|
| 1 | D0.1 to D0.3, D1.1 to D1.6 |
| 2 | D2.1 to D2.3 |
| 3 | D2.4 to D2.6 |
| 4 | D3.1 to D3.3 |
| 5 | D3.4 to D3.6 |
| 6 | D4.1 to D4.2 |
| 7 | D4.3 to D4.6 |
| 8 | D4.7, D4.8, D5.1 to D5.2 |
| 9 | D5.3 to D5.6 |
| 10 | D6.1 to D6.6 |
| 11 | D7.1 to D7.4 |
| 12 | D8.1 to D8.3 |
| 13 | D8.4 to D8.6 |
| 14 | D9.1 to D9.6 |

If a week slips, slide everything after it; do not compress the KEY deliverables to catch up.
