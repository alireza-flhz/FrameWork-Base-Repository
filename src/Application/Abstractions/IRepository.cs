using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Application.Specifications;
using BaseRepository.Domain.Common;
using BaseRepository.Domain.Entities;

namespace BaseRepository.Application.Abstractions;

/// <summary>
/// Add/Update/Remove only stage changes; call <see cref="IUnitOfWork.SaveChangesAsync"/>
/// to persist them.
/// </summary>
public interface IRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<PagedResult<TEntity>> PaginatedListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}
