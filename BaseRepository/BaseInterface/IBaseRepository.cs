using BaseRepository.ViewModels;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BaseRepository.BaseInterface
{
    /// <summary>
    /// Add/Update/Delete only stage changes on the tracked context; call <see cref="SaveAsync"/>
    /// to persist them. This lets callers combine several operations into a single transaction.
    /// </summary>
    public interface IBaseRepository<TModel, in TKey> where TModel : class
    {
        Task<OperationResult<TModel>> AddAsync(TModel model, CancellationToken cancellationToken = default);
        OperationResult<TModel> Update(TModel model);
        Task<OperationResult<TModel>> DeleteAsync(TModel model, CancellationToken cancellationToken = default);
        Task<OperationResult<TModel>> DeleteAsync(TKey id, CancellationToken cancellationToken = default);
        Task<OperationResult<TModel>> DeleteAllAsync(IQueryable<TModel> modelsToDelete, CancellationToken cancellationToken = default);
        Task<OperationResult<TModel>> GetAsync(TKey id, CancellationToken cancellationToken = default);
        Task<OperationResult<IQueryable<TModel>>> GetAllAsQueryable(bool asNoTracking = false);
        Task<OperationResult<IQueryable<TModel>>> AllIncluding(Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>? include = null, bool asNoTracking = false);
        IQueryable<TModel> Paginated(int pageSize, IQueryable<TModel> models, int pageIndex = 1, bool asNoTracking = false);
        Task<OperationResult<TModel>> SaveAsync(CancellationToken cancellationToken = default);
    }
}
