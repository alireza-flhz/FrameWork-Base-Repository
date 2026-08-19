# FrameWork-Base-Repository

A small, generic EF Core repository base class for .NET. Inherit it once per
entity (or use it directly) and you get async CRUD, pagination, eager-loading
includes, and a consistent `OperationResult<T>` wrapper around every
operation — instead of hand-writing the same repository boilerplate in every
new project.

- Targets **.NET 10** / **EF Core 10**.
- Works with *any* `DbContext` — you don't edit the library's source to point
  it at your context, you just pass your context in.
- `Add` / `Update` / `Delete` only stage changes; nothing hits the database
  until you call `SaveAsync()`. This lets you batch several operations into
  one transaction (a small Unit-of-Work pattern).

## Install

Not published to NuGet yet — for now, copy the `BaseRepository` folder into
your solution (or add it as a project reference) and reference it from your
data project:

```
dotnet add reference ../BaseRepository/BaseRepository.csproj
```

## Quick start

### 1. Bring your own `DbContext`

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
}
```

### 2. Define your entity

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

### 3. Expose a repository for it

You can use `BaseRepository<TModel, TKey>` directly, or add your own
interface/class when an entity needs extra queries beyond the base CRUD set:

```csharp
public interface IProductRepository : IBaseRepository<Product, int> { }

public class ProductRepository : BaseRepository<Product, int>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }
}
```

### 4. Register it with DI

```csharp
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
```

### 5. Use it

```csharp
public class ProductService(IProductRepository products)
{
    public async Task<OperationResult<Product>> CreateAsync(Product product)
    {
        var addResult = await products.AddAsync(product);
        if (!addResult.Success)
            return addResult;

        return await products.SaveAsync() is { Success: true }
            ? addResult
            : new OperationResult<Product>(addResult.TableName) { Message = "Save failed" };
    }

    public async Task<IReadOnlyList<Product>> GetPageAsync(int pageIndex, int pageSize)
    {
        var all = await products.GetAllAsQueryable(asNoTracking: true);
        return products.Paginated(pageSize, all.Model!, pageIndex).ToList();
    }
}
```

## API

| Method | Description |
| --- | --- |
| `AddAsync(model)` | Stages an insert. |
| `Update(model)` | Attaches `model` and marks it `Modified`. |
| `DeleteAsync(model)` / `DeleteAsync(id)` | Stages a delete, by entity or by key. |
| `DeleteAllAsync(query)` | Stages a delete for every entity matched by `query`. |
| `GetAsync(id)` | Looks up a single entity by key. |
| `GetAllAsQueryable(asNoTracking)` | Returns the full `DbSet` as an `IQueryable` for further composition. |
| `AllIncluding(include, asNoTracking)` | Same as above, with an `Include(...)` chain applied. |
| `Paginated(pageSize, query, pageIndex, asNoTracking)` | Applies `Skip`/`Take` to any `IQueryable`. Pure in-memory query composition — no `SaveAsync` needed. |
| `SaveAsync()` | Persists every staged change via `SaveChangesAsync`. |

Every method (other than `Paginated`) returns an `OperationResult<T>`:

```csharp
public class OperationResult<T>
{
    public string TableName { get; }
    public T? Model { get; set; }
    public long OperationDate { get; }   // Unix timestamp, set at construction
    public string? Message { get; set; } // populated on failure
    public bool Success { get; set; }
}
```

Check `Success` before trusting `Model`; on failure, `Message` carries the
exception text.

## License

MIT — see [LICENSE](LICENSE).
