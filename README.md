# BaseRepository

[![CI](https://github.com/alireza-flhz/CleanArchitecture-Api-Template/actions/workflows/ci.yml/badge.svg)](https://github.com/alireza-flhz/CleanArchitecture-Api-Template/actions/workflows/ci.yml)

A fully generic Clean Architecture starter for .NET — the "basics every project
needs" (layering, a Result type, pagination, health checks, a test project per
layer, and working register/login with self-service profile management) wired
up once, so a new project starts from a working skeleton instead of from
scratch.

- Targets **.NET 10**.
- Central package version management (`Directory.Packages.props`) — no
  per-project version drift.
- Being built in phases; each phase lands as a working, tested increment.
  See [Roadmap](#roadmap) below for where things stand.

## Start a new project from this

This repo doubles as a `dotnet new` template — scaffolding a project renames
everything (namespaces, project files, the solution) to your chosen name in
one command, instead of cloning and find-and-replacing "BaseRepository"
yourself:

```
dotnet new install <path-to-this-repo-or-its-.nupkg>
dotnet new basecrud -n Acme.Store
cd Acme.Store
dotnet run --project src/Api
```

That's a real, complete, differently-named solution — see
[Template-ization](#template-ization-phase-5) below for what that command
actually does and how it was verified.

## Solution layout

```
src/
  Domain/           BaseRepository.Domain          — zero dependencies
  Application/       BaseRepository.Application      — depends on Domain
  Infrastructure/     BaseRepository.Infrastructure    — depends on Application
  Api/               BaseRepository.Api             — depends on Application + Infrastructure
tests/
  BaseRepository.Domain.UnitTests
  BaseRepository.Application.UnitTests
  BaseRepository.Infrastructure.IntegrationTests
  BaseRepository.Api.FunctionalTests
```

Dependencies only ever point inward (`Api → Application/Infrastructure →
Application → Domain`); `Domain` never references anything else. This is what
lets the persistence provider, or the API framework, be swapped later without
touching business logic.

## Run it

```
dotnet run --project src/Api
```

Then check:
- `GET /health` → 200 OK (ASP.NET Core health checks)
- `GET /` → `{ "service": "BaseRepository.Api", "status": "running" }`
- `GET /openapi/v1.json` → the generated OpenAPI document
- `GET /scalar/v1` → interactive API docs (Scalar)

All four work with zero configuration. `TodoItem` (see
[Template-ization](#template-ization-phase-5)) is real and SQLite-backed
(`app.db`, created automatically), but — like everything routed through
`BaseCrudController` — requires a bearer token. Set a signing key, then get
one from [Auth](#auth-base-not-a-sample) itself instead of hand-crafting one:

```
dotnet user-secrets init --project src/Api
dotnet user-secrets set Jwt:SigningKey "<at least 32 bytes>" --project src/Api
dotnet run --project src/Api

curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" -d "{\"email\":\"me@example.com\",\"password\":\"correct-horse-battery\"}"
# => { "userId": 1, "email": "me@example.com", "token": "...", "expiresAt": "..." }

curl -X POST http://localhost:5000/api/v1/todo-items \
  -H "Authorization: Bearer <the token from above>" \
  -H "Content-Type: application/json" -d "{\"title\":\"buy milk\"}"
```

## Test it

```
dotnet test
```

Every layer has its own test project (unit tests for Domain/Application,
integration tests for Infrastructure, functional tests for the API via
`WebApplicationFactory`).

## What's in Domain today

- `Result` / `Result<T>` (`BaseRepository.Domain.Common`) — a uniform
  success/failure wrapper so handlers and endpoints don't invent their own
  ad-hoc error shapes.
- `PagedResult<T>` (`BaseRepository.Domain.Common`) — page metadata
  (`TotalPages`, `HasNextPage`, `HasPreviousPage`) computed once, reused
  everywhere.
- `DomainException` and its `NotFoundException` / `ConflictException` /
  `BusinessRuleException` / `AuthenticationFailedException` subtypes — the
  vocabulary the API layer translates into HTTP status codes (see
  `GlobalExceptionHandler` under [API](#api-api)).
- `BaseEntity<TKey>` (`BaseRepository.Domain.Entities`) — the base every
  entity derives from. `IAuditableEntity` / `ISoftDelete` are opt-in
  interfaces an entity implements to get automatic audit stamping /
  soft-delete, handled entirely by Infrastructure.

## Persistence (Application + Infrastructure)

- **Specification pattern** (`BaseRepository.Application.Specifications`) —
  derive from `Specification<T>` to describe a filter/include/order/paging
  query without referencing EF Core. `SpecificationEvaluator<T>`
  (Infrastructure) turns it into an `IQueryable<T>`.
- **`IRepository<TEntity, TKey>`** (`BaseRepository.Application.Abstractions`)
  — `GetByIdAsync`, `ListAsync`, `PaginatedListAsync`, `CountAsync`,
  `AnyAsync`, `AddAsync`, `Update`, `Remove`. Add/Update/Remove only stage
  changes; call `IUnitOfWork.SaveChangesAsync()` to persist.
- **`EfRepository<TEntity, TKey>`** (Infrastructure) — the generic EF Core
  implementation. Works with *any* `DbContext`; register it via
  `services.AddPersistence<TContext>(options => options.UseSqlite(...))`
  once your project has one.
- **`BaseDbContext`** (Infrastructure, optional) — inherit it to get a global
  query filter that hides `ISoftDelete` rows automatically.
- **`AuditableEntitySaveChangesInterceptor`** (Infrastructure) — stamps
  `CreatedAt`/`LastModifiedAt` on `IAuditableEntity` entities and turns a
  staged delete of an `ISoftDelete` entity into an update instead of an
  actual row deletion.

All of this is proven end-to-end against a real SQLite database in
`BaseRepository.Infrastructure.IntegrationTests` — see `EfRepositoryTests`.

## CQRS (Application)

- **No MediatR.** MediatR (and AutoMapper) moved to a commercial/RPL-1.5
  license starting v13 — a copyleft-ish license that could force a
  closed-source project built on this template to either open its source or
  buy a license. `BaseRepository.Application.Messaging` is a small
  (~100 line) MIT-equivalent, zero-dependency in-process mediator with the
  same shape (`IRequest<T>`, `IRequestHandler<T,TResponse>`, `ISender`,
  `IPipelineBehavior<T,TResponse>`), so there's no license risk baked into
  the base template. Mapping uses **Mapster** (MIT) instead of AutoMapper for
  the same reason.
- **`LoggingBehavior<,>`** / **`ValidationBehavior<,>`** — registered
  automatically by `AddApplication()`. Validation is via **FluentValidation**
  (Apache-2.0, unaffected by the MediatR/AutoMapper license change) — a
  request with no registered validator just skips validation.
- **Generic CRUD**: `CreateCommand<TEntity,TKey,TDto>`,
  `UpdateCommand<TEntity,TKey,TDto>`, `DeleteCommand<TEntity,TKey>`,
  `GetByIdQuery<TEntity,TKey,TDto>`, `GetListQuery<TEntity,TKey,TDto>` (paged)
  and their handlers, under `BaseRepository.Application.Cqrs`. Failures throw
  the Domain exceptions from Phase 0/1 (`NotFoundException`, ...); success
  returns the DTO/`PagedResult<TDto>` directly — no redundant wrapper, since
  the failure path never returns a value to wrap.
- Register CRUD for one entity with a single call:
  `services.AddCrudHandlers<Product, int, ProductDto>();`. Add a
  `FluentValidation` validator for `CreateCommand<Product,int,ProductDto>`
  (or `UpdateCommand<...>`) if that entity needs write validation — otherwise
  skip it, validation stays optional per entity.

Proven in `BaseRepository.Application.UnitTests` against a hand-rolled
in-memory `IRepository` fake (no database needed to test Application logic)
— mediator dispatch and pipeline ordering, validation pass/fail, and all five
generic handlers including the not-found and pagination-metadata paths.

## API (Api)

- **`BaseCrudController<TEntity, TKey, TDto>`** (`BaseRepository.Api.Controllers`)
  — full REST CRUD (`GET` list w/ paging, `GET {id}`, `POST`, `PUT {id}`,
  `DELETE {id}`) routed straight through the Phase 2 mediator. Derive a
  concrete controller with its own route:
  ```csharp
  [Route("api/products")]
  public class ProductsController : BaseCrudController<Product, int, ProductDto>
  {
      public ProductsController(ISender sender) : base(sender) { }
  }
  ```
  If `TDto` implements `IHasId<TKey>` (`BaseRepository.Application.Cqrs`),
  `Create` returns a proper `Location` header; otherwise it still returns 201,
  just without one.
- **`GlobalExceptionHandler`** (`IExceptionHandler`) — maps `NotFoundException`
  → 404, `ConflictException` → 409, `AuthenticationFailedException` → 401,
  `BusinessRuleException` → 422, `FluentValidation.ValidationException` → 400
  (with a per-property `errors` extension), anything else → 500. All as
  RFC 7807 `ProblemDetails`.
- **OpenAPI**: the built-in `Microsoft.AspNetCore.OpenApi` (`/openapi/v1.json`)
  + **Scalar** (`/scalar/v1`) for an interactive UI — both MIT, no Swashbuckle
  needed.

Proven end-to-end in `BaseRepository.Api.FunctionalTests`: a real sample
entity/DTO/controller wired into the actual `Program.cs` pipeline via a
custom `WebApplicationFactory` (real SQLite, real HTTP, real validation),
covering the full create→read→list→update→delete lifecycle, a 400 on a
failed validator, and a 404 with `application/problem+json` on a missing
entity. This caught a real bug during development: Mapster's in-place
`Adapt(source, destination)` writes through non-public setters too, so
`UpdateCommandHandler`'s `Adapt(entity)` was silently overwriting the
tracked entity's real `Id` with the DTO's default `0`, which made EF Core
reject the update as "changing the key" — fixed by having
`AddCrudHandlers<TEntity,TKey,TDto>()` configure Mapster to ignore `Id` for
that DTO→entity pair.

## Cross-cutting (Phase 4)

- **Structured logging**: Serilog (Apache-2.0) replaces the default provider
  — `UseSerilogRequestLogging()` gives one structured summary line per
  request instead of the default per-middleware noise.
- **JWT auth, secure by default**: `BaseCrudController` now carries
  `[Authorize]` at the class level — every generic CRUD endpoint requires a
  valid bearer token unless a concrete controller opts out with
  `[AllowAnonymous]`. Configure `Jwt:Issuer`/`Jwt:Audience`/`Jwt:SigningKey`
  (e.g. via `dotnet user-secrets` or environment variables — `SigningKey` is
  intentionally blank in `appsettings.json`, never ship a working default
  secret). Health/root/OpenAPI/Scalar stay open with no config at all;
  protected endpoints fail clearly until you configure a key.
- **API versioning**: `Asp.Versioning.*` (MIT) — `BaseCrudController` is
  `[ApiVersion("1.0")]`; route your concrete controllers as
  `api/v{version:apiVersion}/products`. OpenAPI gets one document per
  version (`/openapi/v1.json`, `WithDocumentPerVersion()`).
- **Output caching**: `AddOutputCache()`/`UseOutputCache()` are wired, but
  deliberately **not** applied to `BaseCrudController` by default — caching
  an `[Authorize]`-protected response without varying the cache key by user
  is a real data-leak risk (one user's cached response served to another).
  Apply `[OutputCache]` yourself on endpoints you've actually thought about
  (typically `[AllowAnonymous]` reference-data reads).

Proven in `BaseRepository.Api.FunctionalTests`: a valid bearer token reaches
protected endpoints (200), no token or a garbage token doesn't (401), the
versioned route works, and two rapid calls to a cached public endpoint
return an identical value. Building this surfaced a real bug: the JWT
signing key was read into a local variable *before* `WebApplicationFactory`
merges the test's configuration override into `builder.Configuration`, so
every token failed validation with "no security keys were provided" even
though the key was configured — fixed by reading configuration lazily
inside the `AddJwtBearer` options delegate instead of capturing it into a
variable beforehand, which is also just better practice regardless of tests.

## Auth (base, not a sample)

Register/login is something almost every API needs, so — unlike `TodoItem` —
it's part of the base template, not a sample to delete later.

- **`User`** (Domain) — `Email` + `PasswordHash` + an optional `PhoneNumber`,
  plus `IAuditableEntity`. `AppDbContext` configures a unique index on both
  `Email` and `PhoneNumber` (the latter nullable — NULLs don't collide with
  each other under a unique index, so any number of users can each skip
  setting one).
- **`POST /api/v1/auth/register`** / **`POST /api/v1/auth/login`**
  (`AuthController`, `[AllowAnonymous]`) — both take `{ "email", "password" }`
  and return `{ userId, email, token, expiresAt }`. `RegisterCommand`/
  `LoginCommand` are bespoke `IRequest<AuthResultDto>` handlers (not generic
  CRUD — auth has real business rules), under
  `BaseRepository.Application.Auth`.
- **`IPasswordHasher`** (Application interface, Infrastructure implementation
  via **BCrypt.Net-Next**, MIT) — passwords are never stored or compared in
  plain text.
- **`IJwtTokenGenerator`** (Application interface, Infrastructure
  implementation) — issues a token signed with the same `Jwt:SigningKey`
  `Program.cs` already validates incoming tokens against, read lazily for the
  same reason as the `AddJwtBearer` options delegate.
- Registering with an email already taken → `ConflictException` (409, and a
  DB-level unique-index safety net behind it). Login with a wrong password
  *or* an unknown email both throw the same `AuthenticationFailedException`
  (401) — the response doesn't reveal whether an email is registered.
- Both `AddApplication()` (handlers) and `AddInfrastructure()` (hasher/token
  generator) register Auth automatically — no `AddCrudHandlers<>()`-style
  opt-in call needed, since every project needs it.
- **Email is the only way to register or log in** — a phone number is never a
  replacement identifier. Once signed in, a user can attach/change/clear
  their own phone number via **`PUT /api/v1/auth/me/phone`**
  (`ProfileController`, `{ "phoneNumber": "...", "region": "IR" }` →
  `{ userId, email, phoneNumber }`). Unlike `AuthController`, this one
  requires a bearer token — the user being edited comes from the token's
  claims via **`ICurrentUser`** (`Application` interface, implemented in
  `Api` as `CurrentUser` because it needs `HttpContext`), never from a route
  or body parameter, so nobody can edit someone else's profile this way. A
  number already taken by another user → 409, same as email on register.
  The same pattern (an authenticated `ICurrentUser`-driven command) is how
  you'd add more self-service profile fields later.
- **Any country, not just Iran.** Phone numbers are validated/normalized by
  **`IPhoneNumberValidator`** (`Application` interface, `Infrastructure`
  implementation wrapping **libphonenumber-csharp**, Apache-2.0 — the same
  engine behind Android's own dialer) and stored as **E.164**
  (`+989123456789`, `+16502530000`, ...). Pass a number already in
  international form and leave `region` out, or pass a local-format number
  with its ISO 3166-1 alpha-2 `region` (`"IR"`, `"US"`, `"GB"`, ...) — a
  local-format number with no region is rejected as ambiguous rather than
  guessed at.

Proven in all three test layers: `BaseRepository.Application.UnitTests`
(register/login/phone-update against fake hasher/token generator/current-
user/phone-validator + an in-memory repository), `BaseRepository.Infrastructure.IntegrationTests`
(real BCrypt hashing, real JWT claims/expiry, both unique-index constraints
via a real SQLite `AppDbContext`, and the real `PhoneNumberValidator` against
actual Iranian/US/UK/German/Australian numbers), and
`BaseRepository.Api.FunctionalTests` (`AuthEndToEndTests` / `ProfileEndToEndTests`
— register → login → the issued token reaching a protected endpoint, and
register → update phone (international form, and local-format + region) →
conflict-on-reuse → clear — through the real, unmodified `Program.cs`).

## Utilities (Iranian localization helpers)

A few small, genuinely reusable helpers under `BaseRepository.Domain.Common`
— zero-dependency (calendar conversion uses .NET's own `PersianCalendar`/
`HijriCalendar`, already in the BCL, and the mobile-number check below is a
plain regex) and opt-in, not wired into anything by default. Unlike Auth,
these are regional, not universal — pull in what your project actually
needs. (For phone numbers from *any* country, see `IPhoneNumberValidator`
under [Auth](#auth-base-not-a-sample) instead — this one is Iran-only, on
purpose, for when that's all you need and you'd rather not pull in a bigger
library for it.)

- **`IranianCalendar.ToShamsi`/`FromShamsi`/`ToHijri`/`FromHijri`** — convert
  between `DateTime` (Gregorian) and Shamsi/Hijri `(Year, Month, Day)` tuples.
  Note `HijriCalendar` uses a fixed tabular algorithm, not the official
  Umm al-Qura calendar, so it can be a day off from observation-based dates
  for religious occasions.
- **`IranianNationalCode.IsValid(string)`** — validates an Iranian national
  ID's (کد ملی) checksum digit (and rejects all-same-digit strings, which
  pass the checksum by construction but are never real).
- **`PersianMobileNumber.IsValid(string)`** / **`.Normalize(string)`** —
  Iran-only, zero-dependency: accepts the common prefixed forms (`+98`,
  `0098`, `98`, `0`) and normalizes all of them to the local 11-digit form
  (`09XXXXXXXXX`).
- **FluentValidation rules** (`BaseRepository.Application.Common.Validation`)
  wrap the two validators above for direct use in any `AbstractValidator<T>`:
  `RuleFor(x => x.Phone).PersianMobileNumber();` /
  `RuleFor(x => x.NationalCode).IranianNationalCode();`.

Proven in `BaseRepository.Domain.UnitTests` (round-trip calendar conversions,
a hand-derived valid/invalid national code, every accepted mobile-number
form) and `BaseRepository.Application.UnitTests` (the FluentValidation rules
actually pass/fail through a real validator).

## Template-ization (Phase 5)

- **A real sample, not a fake one.** `TodoItem` (Domain) / `TodoItemDto` +
  `CreateTodoItemValidator` (Application) / `AppDbContext` (Infrastructure) /
  `TodoItemsController` (Api) are wired into the actual, unmodified
  `Program.cs` — SQLite-backed (`app.db`, schema created automatically via
  `EnsureCreated()`, no migrations tooling required to just run it). It's the
  one thing in this repo that isn't meant to stay: once you've added your own
  entities the same way, delete `TodoItem` and its DTO/validator/controller/
  DbSet. Every type carries a doc comment saying so.
- **`AddApplication()` now auto-discovers validators** via
  `FluentValidation.DependencyInjectionExtensions` (Apache-2.0) —
  `AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly)`. Drop an
  `AbstractValidator<...>` next to the DTO it validates; no registration line
  needed any more (this replaced the Phase 2 approach of registering each
  validator by hand).
- **`dotnet new` template**: `.template.config/template.json` at the repo
  root, `sourceName: "BaseRepository"` — scaffolding replaces that string
  everywhere (namespaces, `.csproj`/`.sln` file names, even string literals
  like the `/` endpoint's `service` field) with whatever name you pass to
  `-n`. `template-pack/BaseRepository.Template.csproj` packs it as an
  installable `.nupkg` for distribution (not published anywhere yet — that's
  a deliberate choice for someone with publish rights to make, not this
  commit's job).

Verified for real, three different ways: (1) `dotnet new install` on this
repo folder directly, scaffold `Acme.Store`, `dotnet build` + `dotnet test`
→ 49/49 passing, `dotnet run` → full TodoItem CRUD lifecycle over real HTTP;
(2) same again with an unrelated name (`Contoso.Widgets`) to rule out a
lucky one-off; (3) `dotnet pack` the template project, install *from the
resulting `.nupkg`* (the actual distribution path, not just a folder
install), scaffold `Northwind.Api`, build clean. Building this surfaced a
real bug: once `Program.cs` itself wired up `AppDbContext`, *every*
`WebApplicationFactory`-based test — even ones with nothing to do with
`TodoItem` — started spinning it up too, and every parallel test class got
its own factory instance pointed at the same relative `app.db` file,
causing intermittent SQLite collisions when the full suite ran together
(invisible running any one test project alone). Fixed by having the test
factories override `ConnectionStrings:Default` to a unique temp-file path
per factory instance.

## CI/CD (Phase 6)

- **`.github/workflows/ci.yml`** — on every push/PR to `master` (and
  manually via `workflow_dispatch`): restore, `dotnet build --configuration
  Release`, then `dotnet test` across all four test projects, with TRX
  results uploaded as a build artifact. A second job (`template-smoke-test`)
  regression-guards the `dotnet new` mechanism itself: install this repo as
  a template, scaffold a throwaway project from it, and build + test *that*
  — so a change that breaks templating (not just the base solution) fails
  CI too.
- **`.github/workflows/publish-template.yml`** — packs and pushes
  `template-pack/BaseRepository.Template.csproj` to NuGet.org. Triggered
  only by a `v*.*.*` tag or manual dispatch, never a regular push. It
  needs a `NUGET_API_KEY` repository secret to actually publish anything;
  without one it fails cleanly at the push step. Nobody has configured that
  secret or published this template anywhere yet — that's a deliberate
  call left to whoever owns this repo and wants to make it public.

## Roadmap

- [x] **Phase 0 — Foundations & solution skeleton.** Layered projects, central
      package management, `Result<T>`/`PagedResult<T>`, base exceptions,
      health-check endpoint, a test project per layer.
- [x] **Phase 1 — Domain & persistence core.** `BaseEntity<TKey>`,
      auditing/soft-delete interfaces, Specification pattern, generic
      `IRepository<T,TKey>` + EF Core implementation + `UnitOfWork`.
- [x] **Phase 2 — Generic CQRS.** Custom mediator (see above), generic
      Create/Update/Delete/GetById/GetList handlers, FluentValidation,
      Mapster, pipeline behaviors — adding an entity needs a DTO + one
      `AddCrudHandlers<...>()` call.
- [x] **Phase 3 — Generic API.** `BaseCrudController<T,TKey,TDto>`, global
      exception handling → `ProblemDetails`, OpenAPI + Scalar UI, paging from
      the query string.
- [x] **Phase 4 — Cross-cutting.** Serilog structured logging, JWT auth
      (secure by default) + policy-based authorization, API versioning,
      output caching.
- [x] **Phase 5 — Template-ization.** `dotnet new` template (`sourceName`
      rename of everything), a real wired `TodoItem` sample, auto-discovered
      validators.
- [x] **Phase 6 — CI/CD.** Full test suite (+ a template scaffolding smoke
      test) wired into GitHub Actions; a dormant, secret-gated NuGet publish
      workflow. *(this commit)*

Generic CRUD (Phases 1-3) covers most simple/master-data entities. Anything
with real business rules is expected to get a bespoke Application handler or
Api controller — the generic path is the default, not a mandate.

**Since Phase 6**, two more things landed as base (not phased, since they
weren't part of the original roadmap, but real and tested the same way as
everything above): [Auth](#auth-base-not-a-sample) — register/login,
password hashing, JWT issuing, and a self-service profile endpoint for an
international (not Iran-only) phone number — and the
[Iranian localization helpers](#utilities-iranian-localization-helpers)
(calendar conversion, national-code/mobile-number validators). Test count
has grown accordingly (135 as of the latest addition) — run `dotnet test`
for the current number rather than trusting any figure quoted above, since
those are point-in-time notes from when each phase/feature was built.

## License

MIT — see [LICENSE](LICENSE).
