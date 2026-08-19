using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Application.Abstractions;
using BaseRepository.Application.Specifications;
using BaseRepository.Domain.Common;
using BaseRepository.Domain.Entities;
using BaseRepository.Infrastructure.Persistence.Specifications;
using Microsoft.EntityFrameworkCore;

namespace BaseRepository.Infrastructure.Persistence;

public class EfRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
{
    private readonly DbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public EfRepository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync(new object?[] { id }, cancellationToken);

    public virtual async Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        => await Evaluate(specification).FirstOrDefaultAsync(cancellationToken);

    public virtual async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        => await Evaluate(specification).ToListAsync(cancellationToken);

    public virtual async Task<PagedResult<TEntity>> PaginatedListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        var totalCount = await Evaluate(specification, evaluatePaging: false).CountAsync(cancellationToken);
        var items = await Evaluate(specification).ToListAsync(cancellationToken);

        var pageSize = specification.IsPagingEnabled ? specification.Take : totalCount;
        var pageIndex = specification.IsPagingEnabled && pageSize > 0
            ? (specification.Skip / pageSize) + 1
            : 1;

        return new PagedResult<TEntity>(items, totalCount, pageIndex, pageSize);
    }

    public virtual async Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        => await Evaluate(specification, evaluatePaging: false).CountAsync(cancellationToken);

    public virtual async Task<bool> AnyAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        => await Evaluate(specification, evaluatePaging: false).AnyAsync(cancellationToken);

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    public virtual void Update(TEntity entity)
    {
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public virtual void Remove(TEntity entity) => _dbSet.Remove(entity);

    private IQueryable<TEntity> Evaluate(ISpecification<TEntity> specification, bool evaluatePaging = true)
        => SpecificationEvaluator<TEntity>.GetQuery(_dbSet.AsQueryable(), specification, evaluatePaging);
}
