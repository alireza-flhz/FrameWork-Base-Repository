using BaseRepository.ViewModels;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseRepository.BaseInterface
{
    public interface IBaseRepository<TModel, in TKey> : IDisposable where TModel : class
    {
        Task<OperationResult> AddAsync(TModel model);
        TModel Update(TModel model);
        Task<OperationResult> DeleteAsync(TModel model);
        Task<OperationResult> DeleteAllAsync(IQueryable<TModel> deleteModels);
        Task<OperationResult> DeleteAsync(TKey id);
        Task<TModel> GetAsync(TKey id);
        IQueryable<TModel> GetAllAsQueryable(bool asNoTracking = false);
        IQueryable<TModel> GetAllAsQueryable(Func<TModel, bool> predicate, bool asNoTracking = false);
        IQueryable<TModel> AllIncluding(Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>> include = null, bool asNoTracking = false);
        IQueryable<TModel> Paginated(int pageSize, IQueryable<TModel> Models, int pageIndex = 1, bool asNoTracking = false);
        Task<OperationResult> SaveAsync();
        TModel GetInclude(Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>> include, Func<TModel, bool> predicate);
    }
}
