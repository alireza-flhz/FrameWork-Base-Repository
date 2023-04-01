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
        Task<OperationResult<TModel>> AddAsync(TModel model);
        OperationResult<TModel> Update(TModel model);
        Task<OperationResult<TModel>> DeleteAsync(TModel model);
        Task<OperationResult<TModel>> DeleteAllAsync(IQueryable<TModel> deleteModels);
        Task<OperationResult<TModel>> DeleteAsync(TKey id);
        OperationResult<TModel> GetAsync(TKey id);
        Task<OperationResult<IQueryable<TModel>>> GetAllAsQueryable(bool asNoTracking = false);
        Task<OperationResult<IQueryable<TModel>>> AllIncluding(Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>> include = null, bool asNoTracking = false);
        Task<OperationResult<IQueryable<TModel>>> Paginated(int pageSize, IQueryable<TModel> Models, int pageIndex = 1, bool asNoTracking = false);
        Task<OperationResult<TModel>> SaveAsync();
    }
}
