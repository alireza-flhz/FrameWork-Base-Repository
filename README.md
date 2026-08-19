# BaseRepository

A fully generic Clean Architecture starter for .NET — the "basics every project
needs" (layering, a Result type, pagination, health checks, a test project per
layer) wired up once, so a new project starts from a working skeleton instead
of from scratch.

- Targets **.NET 10**.
- Central package version management (`Directory.Packages.props`) — no
  per-project version drift.
- Being built in phases; each phase lands as a working, tested increment.
  See [Roadmap](#roadmap) below for where things stand.

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

All four work with zero configuration. Any entity you add via
`BaseCrudController` requires a bearer token — set
`dotnet user-secrets set Jwt:SigningKey "<at least 32 bytes>"` (and
`Jwt:Issuer`/`Jwt:Audience` if you want non-default values) before those
routes will accept requests.

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
  `BusinessRuleException` subtypes — the vocabulary the API layer will
  translate into HTTP status codes starting Phase 3.
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
  → 404, `ConflictException` → 409, `BusinessRuleException` → 422,
  `FluentValidation.ValidationException` → 400 (with a per-property `errors`
  extension), anything else → 500. All as RFC 7807 `ProblemDetails`.
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
      output caching. *(this commit)*
- [ ] **Phase 5 — Template-ization.** Package as a `dotnet new` template so a
      new project is one command, not a copy-paste.
- [ ] **Phase 6 — CI/CD.** Full test suite wired into GitHub Actions, optional
      NuGet/template publish.

Generic CRUD (Phases 1-3) covers most simple/master-data entities. Anything
with real business rules is expected to get a bespoke Application handler or
Api controller — the generic path is the default, not a mandate.

## License

MIT — see [LICENSE](LICENSE).
