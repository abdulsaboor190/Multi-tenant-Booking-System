# BookWise: Executable Project Plan

A phase-by-phase build plan for the multi-tenant booking system (ASP.NET Core 10, EF Core, PostgreSQL, Hangfire, Next.js).

**How to use this file:** work top to bottom. Each phase has a goal, tasks you can tick off, a deliverable, and a **Definition of Done (DoD)**. Do not start the next phase until the DoD is met. Copy this file into your repo as `docs/PLAN.md` and tick boxes as you go.

**Assumed pace:** 10 to 15 hours per week. Time estimates scale accordingly.

---

## Overview

| Phase | Name | Est. time | Output |
|---|---|---|---|
| 0 | Environment and repository | 1 to 2 days | Tools installed, repo on GitHub |
| 1 | Foundation | 1 week | Running API, database, logging, CI skeleton |
| 2 | Tenancy and authentication | 1.5 weeks | Tenant sign-up, JWT login, isolated data |
| 3 | Core domain | 1.5 weeks | Services, staff, working hours, time off |
| 4 | Booking engine | 2 weeks | Availability, bookings, no double-booking |
| 5 | Background jobs and email | 1 week | Confirmation and reminder emails |
| 6 | Hardening | 1 week | Policies, rate limiting, pagination, audit |
| 7 | Testing and CI | 1 week | 70 to 80% coverage on core logic, green CI |
| 8 | Frontend | 2.5 weeks | Customer booking flow and owner dashboard |
| 9 | Deploy and documentation | 1 week | Live demo, polished README |
| 10 | Stretch (optional) | open | Extra features after launch |

**Total:** roughly 13 to 15 weeks. The backend (Phases 1 to 7) is the part that gets you hired, so it comes first.

**Compressed path (about 8 weeks):** skip Phase 10, trim Phase 6 to rate limiting and pagination, and build a minimal frontend (customer booking page plus a basic owner list view).

---

## Working Rules

1. **Git discipline:** one branch per phase (`phase-2-auth`), merge to `main` through a pull request you review yourself. Commit small and often, with clear messages.
2. **Keep `main` always green:** it builds and tests pass.
3. **Test as you build,** not at the end. The two tests that matter most (tenant isolation and concurrent booking) get written the moment their feature exists.
4. **Write down decisions.** Keep `docs/DECISIONS.md` and add a short entry whenever you choose something non-obvious (for example "why a shared schema for tenancy"). This becomes your README's best section.
5. **Scope guard:** anything not in the current phase goes into a `Later` list, not into the code.
6. **Weekly rhythm:** plan (15 min) at the start of the week, build, then a short written review at the end (what shipped, what blocked, what next).

---

## Phase 0: Environment and Repository

**Goal:** a working machine and an empty, well-organized repository.

### Tasks
- [ ] Install .NET 10 SDK; verify with `dotnet --version`
- [ ] Install an editor (Visual Studio Community, or VS Code with C# Dev Kit)
- [ ] Install Git; configure name and email
- [ ] Install Docker Desktop (enable WSL 2 and virtualization if prompted); verify with `docker run hello-world`
- [ ] Install DBeaver or pgAdmin
- [ ] Install the EF CLI: `dotnet tool install --global dotnet-ef`
- [ ] Run a throwaway `dotnet new webapi` and confirm it runs, then delete it
- [ ] Create a GitHub repo named `bookwise` with a `.gitignore` (VisualStudio template) and a `README.md`

### Deliverable
Empty repo cloned locally; every tool verified.

### DoD
`dotnet --version`, `git --version`, and `docker --version` all print versions, and the throwaway API ran successfully.

---

## Phase 1: Foundation

**Goal:** a clean layered solution that runs, talks to PostgreSQL, logs properly, and builds in CI.

### Tasks

**1.1 Solution structure**
```bash
mkdir -p src tests docs
dotnet new sln -n BookWise            # .NET 10 creates BookWise.slnx; that is fine

dotnet new classlib -n BookWise.Domain         -o src/BookWise.Domain
dotnet new classlib -n BookWise.Application    -o src/BookWise.Application
dotnet new classlib -n BookWise.Infrastructure -o src/BookWise.Infrastructure
dotnet new webapi   -n BookWise.Api            -o src/BookWise.Api
dotnet new xunit    -n BookWise.UnitTests        -o tests/BookWise.UnitTests
dotnet new xunit    -n BookWise.IntegrationTests -o tests/BookWise.IntegrationTests

dotnet sln add src/BookWise.Domain src/BookWise.Application src/BookWise.Infrastructure src/BookWise.Api
dotnet sln add tests/BookWise.UnitTests tests/BookWise.IntegrationTests

# Dependency rule: Domain depends on nothing
dotnet add src/BookWise.Application reference src/BookWise.Domain
dotnet add src/BookWise.Infrastructure reference src/BookWise.Application
dotnet add src/BookWise.Api reference src/BookWise.Application src/BookWise.Infrastructure
dotnet add tests/BookWise.UnitTests reference src/BookWise.Domain src/BookWise.Application
dotnet add tests/BookWise.IntegrationTests reference src/BookWise.Api
```
- [ ] Delete template boilerplate (sample controllers, weather forecast)
- [ ] Add `Directory.Build.props` with `Nullable` enabled, `TreatWarningsAsErrors` true, and a shared `LangVersion`

**1.2 Local services with Docker Compose**
- [ ] Create `docker-compose.yml` with PostgreSQL and Mailpit:
```yaml
services:
  db:
    image: postgres:17
    environment:
      POSTGRES_USER: bookwise
      POSTGRES_PASSWORD: bookwise_dev
      POSTGRES_DB: bookwise
    ports: ["5432:5432"]
    volumes: [pgdata:/var/lib/postgresql/data]
  mailpit:
    image: axllent/mailpit
    ports: ["1025:1025", "8025:8025"]   # SMTP and web UI
volumes:
  pgdata:
```
- [ ] `docker compose up -d`, then connect with DBeaver to confirm

**1.3 Database wiring**
- [ ] Install `Npgsql.EntityFrameworkCore.PostgreSQL` and `Microsoft.EntityFrameworkCore.Design`
- [ ] Create `AppDbContext` in Infrastructure; register it in `Program.cs`
- [ ] Put the connection string in `appsettings.Development.json`; keep real secrets out of Git (use User Secrets)
- [ ] Enable the `btree_gist` extension in the first migration (needed in Phase 4)
- [ ] Create the first migration and apply it: `dotnet ef migrations add Initial -p src/BookWise.Infrastructure -s src/BookWise.Api`

**1.4 Cross-cutting basics**
- [ ] Serilog with console output and request logging
- [ ] Global exception handling returning RFC 7807 `ProblemDetails`
- [ ] Health check endpoint `/health` that checks the database
- [ ] OpenAPI/Swagger UI in Development
- [ ] FluentValidation registered, with a filter or endpoint filter that returns `400` with errors

**1.5 CI skeleton**
- [ ] `.github/workflows/ci.yml`: restore, build, test on every push and pull request

### Deliverable
`docker compose up -d` then `dotnet run` gives a running API with a green `/health` and a green CI badge.

### DoD
A fresh clone can run the API using only the README steps. CI passes.

---

## Phase 2: Tenancy and Authentication

**Goal:** businesses can register, users can log in, and every query is automatically scoped to the current tenant.

### Tasks

**2.1 Domain and schema**
- [ ] Entities: `Tenant`, `User`, `RefreshToken`
- [ ] `ITenantOwned` interface with `TenantId`
- [ ] Password hashing (ASP.NET Core `PasswordHasher<T>` or BCrypt.Net-Next); never store plain passwords
- [ ] Migration

**2.2 Tenant context**
- [ ] `ITenantContext` (scoped) holding the current `TenantId`
- [ ] Middleware that sets it from the JWT `tenant_id` claim (for public booking pages, from the tenant slug in the route)
- [ ] EF Core **global query filter** on every `ITenantOwned` entity
- [ ] Override `SaveChangesAsync` to stamp `TenantId` on new entities and reject changes to other tenants' rows

**2.3 Authentication**
- [ ] `POST /api/auth/register-tenant` creates Tenant and Owner user in one transaction
- [ ] `POST /api/auth/login` returns an access token (15 minutes) and a refresh token
- [ ] `POST /api/auth/refresh` with **rotation**: the old refresh token is revoked, a new one is issued
- [ ] Store refresh tokens **hashed**, not in plain text
- [ ] `POST /api/auth/logout` revokes the refresh token
- [ ] JWT bearer configured with issuer, audience, signing key from configuration

**2.4 The first important test**
- [ ] Integration test (Testcontainers + `WebApplicationFactory`): create tenants A and B, add data to each, and prove A cannot read, update, or delete B's data

### Deliverable
Register, log in, refresh, and call a protected endpoint, with isolation proven by a test.

### DoD
The two-tenant isolation test passes and is part of CI.

---

## Phase 3: Core Domain

**Goal:** a tenant can model its business: services, staff, schedules.

### Tasks
- [ ] Entities and migrations: `Staff`, `Service`, `StaffService`, `WorkingHours`, `TimeOff`
- [ ] Endpoints (Owner only unless noted):
  - [ ] `POST/GET/PUT/DELETE /api/services` (soft delete or deactivate)
  - [ ] `POST/GET/PUT /api/staff`, plus invite flow (create user with a temporary password or invitation token)
  - [ ] `PUT /api/staff/{id}/working-hours` (replace weekly schedule)
  - [ ] `POST/GET/DELETE /api/staff/{id}/time-off`
  - [ ] `PUT /api/staff/{id}/services` (which services each person performs)
- [ ] Validation rules: duration greater than 0, end time after start time, no overlapping working-hour blocks on the same day
- [ ] Pagination, sorting, and filtering on list endpoints (shared helper, built once)
- [ ] Unit tests for the validation rules

### Deliverable
An owner can fully configure a business through the API (verified in Swagger).

### DoD
All endpoints documented in OpenAPI, validated, tenant-scoped, and covered by at least basic tests.

---

## Phase 4: Booking Engine

**Goal:** the heart of the product. Correct availability, correct bookings, no double-booking.

### Tasks

**4.1 Availability calculation (pure domain logic)**
- [ ] Domain service `AvailabilityCalculator` with **no database access**: inputs are working hours, time off, existing bookings, service duration, buffer, and the tenant time zone; output is a list of free slots
- [ ] Store all times in UTC; convert with the tenant's IANA time zone (consider NodaTime)
- [ ] Handle edge cases: slots at the day boundary, back-to-back bookings, time off mid-day, daylight-saving changes, past times
- [ ] **Heavy unit tests** here (aim for 20+ cases)
- [ ] `GET /api/availability?serviceId=&staffId=&date=`

**4.2 Bookings**
- [ ] Entities and migration: `Booking`, `BookingHistory`, `Customer`
- [ ] Status machine in the domain: `Pending -> Confirmed -> Completed`, plus `Cancelled` and `NoShow`; invalid transitions throw
- [ ] `POST /api/bookings`, `POST /api/bookings/{id}/cancel`, `POST /api/bookings/{id}/reschedule`, `GET /api/bookings` (filters and pagination)
- [ ] Customer registration and login scoped to the tenant

**4.3 Double-booking prevention (the key feature)**
- [ ] Add the exclusion constraint in a migration:
```sql
ALTER TABLE bookings
ADD CONSTRAINT no_overlapping_bookings
EXCLUDE USING gist (
  staff_id WITH =,
  tstzrange(start_time, end_time) WITH &&
) WHERE (status <> 'Cancelled');
```
- [ ] Catch the PostgreSQL exclusion violation (SQLSTATE `23P01`) and return `409 Conflict`
- [ ] Reschedule runs inside one transaction so the old slot is only released if the new one succeeds
- [ ] **Concurrency integration test:** fire 20 parallel requests for the same slot; assert exactly one `201` and nineteen `409`

### Deliverable
A customer can see free slots, book one, and nobody else can take it, even under parallel load.

### DoD
The concurrency test and the availability unit tests pass in CI. You can explain, out loud, why the constraint beats "check then insert".

---

## Phase 5: Background Jobs and Email

**Goal:** reliable asynchronous work: confirmations and reminders.

### Tasks
- [ ] Install `Hangfire.AspNetCore` and `Hangfire.PostgreSql`; protect the `/hangfire` dashboard (Owner or admin only, never public)
- [ ] `IEmailSender` abstraction in Application; MailKit implementation in Infrastructure; point SMTP to Mailpit in development
- [ ] Email templates (simple HTML): booking confirmation, reminder, cancellation
- [ ] On booking creation: enqueue a confirmation email (fire-and-forget) and schedule a reminder 24 hours before start
- [ ] Store the Hangfire job id on the booking; on cancel or reschedule, delete the old reminder and schedule a new one
- [ ] Jobs carry `TenantId` and re-establish tenant context when they run
- [ ] Jobs are **idempotent** (a repeated run does not send a duplicate); mark `ReminderSentAt`
- [ ] Recurring job: expire `Pending` bookings older than 15 minutes
- [ ] Configure retries with backoff; log failures with the tenant and booking id

### Deliverable
Create a booking and see the confirmation in Mailpit (`http://localhost:8025`); a reminder is scheduled in the Hangfire dashboard.

### DoD
Cancelling a booking removes its reminder, and this is covered by a test.

---

## Phase 6: Hardening

**Goal:** the difference between a demo and a product.

### Tasks
- [ ] **Policy-based authorization** with custom requirements and handlers:
  - [ ] `CanManageStaff`, `CanViewBooking` (owner, assigned staff, or booking owner), `CanCancelBooking` (respects the tenant's cancellation window)
  - [ ] Resource-based checks using `IAuthorizationService`
- [ ] Tenant settings: cancellation window, booking lead time, maximum advance booking days; enforce in the domain
- [ ] ASP.NET Core **rate limiting** on login, register, and booking creation
- [ ] Consistent pagination, sorting, and filtering across all list endpoints
- [ ] Booking audit trail (`BookingHistory` written on every status change, with who and when)
- [ ] Review: no endpoint returns another tenant's data or leaks internals in error messages
- [ ] Add useful database indexes (composite, starting with `TenantId`) and check slow queries with `EXPLAIN`
- [ ] CORS configured for the frontend origin only
- [ ] Security headers and HTTPS redirection enabled

### Deliverable
Authorization and settings are enforced everywhere, and abuse is throttled.

### DoD
A written checklist in `docs/SECURITY.md` listing each protection and the test or code that proves it.

---

## Phase 7: Testing and CI

**Goal:** confidence and proof.

### Tasks
- [ ] Unit tests: availability, status machine, policy handlers, settings rules
- [ ] Integration tests with Testcontainers:
  - [ ] Tenant isolation (from Phase 2)
  - [ ] Concurrent booking (from Phase 4)
  - [ ] Refresh token rotation and reuse rejection
  - [ ] Reminder removed on cancel
  - [ ] Authorization failures return `403`/`401` correctly
- [ ] Code coverage report (Coverlet) published in CI; target 70 to 80% on Domain and Application
- [ ] CI pipeline: build, test, coverage, plus a Docker image build
- [ ] Add a coverage badge and a build badge to the README

### Deliverable
A green pipeline with visible coverage.

### DoD
All tests pass from a clean clone using `dotnet test` (Docker running).

---

## Phase 8: Frontend

**Goal:** a usable interface that makes the backend visible to recruiters.

### Tasks
- [ ] Next.js (TypeScript) app with Tailwind, TanStack Query, React Hook Form + Zod
- [ ] Generate a typed API client from the OpenAPI document (for example `openapi-typescript`)
- [ ] Auth: login, register tenant, token refresh handling
- [ ] **Customer flow** (public, per tenant slug): choose service, then staff (or "any"), then date and time slot, then confirm, then see the confirmation page
- [ ] **Owner dashboard:** services, staff, working hours editor, today's bookings, cancel and reschedule
- [ ] **Staff view:** calendar of own bookings (FullCalendar)
- [ ] Good states: loading, empty, errors (including the `409` "slot just taken" message with a refreshed slot list)
- [ ] Responsive layout; test on a phone-sized viewport

### Deliverable
A person can complete the full booking journey in the browser.

### DoD
Record a 60 to 90 second screen capture of the complete flow (it goes in the README).

---

## Phase 9: Deployment and Documentation

**Goal:** a live link and a README that sells the project.

### Tasks
- [ ] Production `Dockerfile` for the API (multi-stage build)
- [ ] Deploy API and PostgreSQL (Azure, Railway, Render, or Fly.io) and the frontend (Vercel)
- [ ] Production configuration: secrets in environment variables, migrations applied on release, CORS for the real frontend URL
- [ ] Seed a **demo tenant** with sample staff, services, and bookings, with demo credentials in the README
- [ ] CD: deploy automatically from `main` after tests pass
- [ ] README with: pitch, screenshot/GIF, live demo link, architecture diagram, tenant isolation explained, double-booking prevention explained, design decisions, run-locally instructions, test instructions, limitations
- [ ] Add the project to your resume, LinkedIn, and GitHub pinned repositories

### Deliverable
A public URL a recruiter can open and use in under a minute.

### DoD
Someone who has never seen the project can understand it from the README in three minutes and try the demo without help.

---

## Phase 10: Stretch (Only After Phase 9)

Pick one at a time; each is a separate resume bullet.

- [ ] Recurring appointments
- [ ] Waitlist with notification when a slot opens
- [ ] PostgreSQL Row-Level Security as a second isolation layer
- [ ] SMS reminders (Twilio)
- [ ] Stripe payments (test mode) with webhook handling and idempotency
- [ ] OpenTelemetry tracing and a Grafana dashboard
- [ ] Redis caching for availability queries
- [ ] Calendar export (`.ics`) and Google Calendar sync
- [ ] Platform admin panel with usage statistics

---

## Progress Tracker

| Phase | Status | Started | Finished | Notes |
|---|---|---|---|---|
| 0 | Not started | | | |
| 1 | Not started | | | |
| 2 | Not started | | | |
| 3 | Not started | | | |
| 4 | Not started | | | |
| 5 | Not started | | | |
| 6 | Not started | | | |
| 7 | Not started | | | |
| 8 | Not started | | | |
| 9 | Not started | | | |

---

## Risk Register

| Risk | Likelihood | Mitigation |
|---|---|---|
| Scope creep | High | The `Later` list; stretch features only after Phase 9 |
| Time-zone bugs | Medium | UTC storage, heavy availability unit tests, consider NodaTime |
| Stalling on the frontend | Medium | Finish the backend first; keep UI simple; use a component library if needed |
| Docker or WSL problems on Windows | Medium | Sort this out in Phase 0; fall back to a native PostgreSQL install temporarily |
| Losing momentum | Medium | Weekly review, small commits, one visible win each week |
| Over-engineering | Medium | No CQRS, no microservices, no event sourcing; plain application services |

---

## Interview Talking Points (Collect as You Build)

By the end you should be able to answer, with real code to point at:

1. How do you keep one business's data away from another's, and how did you test it?
2. Two customers click the same slot at the same moment. What happens, and why?
3. Why an exclusion constraint instead of an application-level check or a lock?
4. How do refresh-token rotation and reuse detection work?
5. How do you make background jobs idempotent?
6. How did you handle time zones?
7. What would you change to scale this to 10,000 tenants?

Write your answers in `docs/DECISIONS.md` as you go; they are your interview preparation.
