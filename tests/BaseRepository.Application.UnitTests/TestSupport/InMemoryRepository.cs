using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Application.Abstractions;
using BaseRepository.Application.Specifications;
using BaseRepository.Domain.Common;
using BaseRepository.Domain.Entities;

namespace BaseRepository.Application.UnitTests.TestSupport;

/// <summary>
/// A hand-rolled in-memory fake of IRepository, so Application-layer tests exercise the
/// generic CQRS handlers without touching EF Core or a database (that's Infrastructure's job).
/// </summary>
public class InMemoryRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : BaseEntity<TKey>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TEntity> _store = new();

    public Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.TryGetValue(id, out var entity) ? entity : null);

    public Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        => Task.FromResult(Filtered(specification).FirstOrDefault());

    public Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<TEntity>>(_store.Values.ToList());

    public Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<TEntity>>(Paged(specification).ToList());

    public Task<PagedResult<TEntity>> PaginatedListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        var filtered = Filtered(specification).ToList();
        var items = Paged(specification).ToList();

        var pageSize = specification.IsPagingEnabled ? specification.Take : filtered.Count;
        var pageIndex = specification.IsPagingEnabled && pageSize > 0
            ? (specification.Skip / pageSize) + 1
            : 1;

        return Task.FromResult(new PagedResult<TEntity>(items, filtered.Count, pageIndex, pageSize));
    }

    public Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        => Task.FromResult(Filtered(specification).Count());

    public Task<bool> AnyAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        => Task.FromResult(Filtered(specification).Any());

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _store[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public void Update(TEntity entity) => _store[entity.Id] = entity;

    public void Remove(TEntity entity) => _store.Remove(entity.Id);

    private IEnumerable<TEntity> Filtered(ISpecification<TEntity> specification)
    {
        var query = _store.Values.AsQueryable();

        if (specification.Criteria is not null)
            query = query.Where(specification.Criteria);

        if (specification.OrderBy is not null)
            query = query.OrderBy(specification.OrderBy);
        else if (specification.OrderByDescending is not null)
            query = query.OrderByDescending(specification.OrderByDescending);

        return query;
    }

    private IEnumerable<TEntity> Paged(ISpecification<TEntity> specification)
    {
        var query = Filtered(specification);
        return specification.IsPagingEnabled ? query.Skip(specification.Skip).Take(specification.Take) : query;
    }
}
