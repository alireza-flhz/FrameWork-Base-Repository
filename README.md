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

## Roadmap

- [x] **Phase 0 — Foundations & solution skeleton.** Layered projects, central
      package management, `Result<T>`/`PagedResult<T>`, base exceptions,
      health-check endpoint, a test project per layer. *(this commit)*
- [ ] **Phase 1 — Domain & persistence core.** `BaseEntity<TKey>`,
      auditing/soft-delete interfaces, Specification pattern
      (`ISpecification<T>`), generic `IRepository<T,TKey>` in Application with
      an EF Core implementation + `UnitOfWork` in Infrastructure.
- [ ] **Phase 2 — Generic CQRS.** MediatR-based generic
      Create/Update/Delete/GetById/GetList handlers, FluentValidation,
      mapping, pipeline behaviors — adding an entity should only require a DTO.
- [ ] **Phase 3 — Generic API.** `BaseController<T,...>`, global exception
      handling → `ProblemDetails`, OpenAPI/Swagger, pagination/filtering from
      the query string.
- [ ] **Phase 4 — Cross-cutting.** Structured logging, auth scaffolding
      (JWT + policies), versioning, caching.
- [ ] **Phase 5 — Template-ization.** Package as a `dotnet new` template so a
      new project is one command, not a copy-paste.
- [ ] **Phase 6 — CI/CD.** Full test suite wired into GitHub Actions, optional
      NuGet/template publish.

Generic CRUD (Phases 1-3) covers most simple/master-data entities. Anything
with real business rules is expected to get a bespoke Application handler or
Api controller — the generic path is the default, not a mandate.

## License

MIT — see [LICENSE](LICENSE).
