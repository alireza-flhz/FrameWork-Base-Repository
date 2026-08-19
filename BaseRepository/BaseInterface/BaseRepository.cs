using BaseRepository.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BaseRepository.BaseInterface
{
    public class BaseRepository<TModel, TKey> : IBaseRepository<TModel, TKey> where TModel : class
    {
        private readonly DbContext _context;
        private readonly DbSet<TModel> _model;
        private readonly string _tableName;

        public BaseRepository(DbContext context)
        {
            _context = context;
            _model = _context.Set<TModel>();
            _tableName = typeof(TModel).Name;
        }

        public virtual async Task<OperationResult<TModel>> AddAsync(TModel model, CancellationToken cancellationToken = default)
        {
            var res = new OperationResult<TModel>(_tableName);
            try
            {
                await _model.AddAsync(model, cancellationToken);
                res.Success = true;
                res.Model = model;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
            return res;
        }

        public virtual OperationResult<TModel> Update(TModel model)
        {
            var res = new OperationResult<TModel>(_tableName);
            try
            {
                _model.Attach(model);
                _context.Entry(model).State = EntityState.Modified;
                res.Success = true;
                res.Model = model;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
            return res;
        }

        public virtual Task<OperationResult<TModel>> DeleteAsync(TModel model, CancellationToken cancellationToken = default)
        {
            var res = new OperationResult<TModel>(_tableName);
            try
            {
                _model.Remove(model);
                res.Success = true;
                res.Model = model;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
            return Task.FromResult(res);
        }

        public virtual async Task<OperationResult<TModel>> DeleteAllAsync(IQueryable<TModel> modelsToDelete, CancellationToken cancellationToken = default)
        {
            var res = new OperationResult<TModel>(_tableName);
            try
            {
                var entities = await modelsToDelete.ToListAsync(cancellationToken);
                _model.RemoveRange(entities);
                res.Success = true;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
            return res;
        }

        public virtual async Task<OperationResult<TModel>> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var res = new OperationResult<TModel>(_tableName);
            try
            {
                var getResult = await GetAsync(id, cancellationToken);
                if (getResult.Model is null)
                {
                    res.Message = "Model not found";
                    return res;
                }

                _model.Remove(getResult.Model);
                res.Success = true;
                res.Model = getResult.Model;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
            return res;
        }

        public virtual async Task<OperationResult<TModel>> GetAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var res = new OperationResult<TModel>(_tableName);
            try
            {
                var model = await _model.FindAsync(new object?[] { id }, cancellationToken);
                if (model is not null)
                {
                    res.Success = true;
                    res.Model = model;
                }
                else
                {
                    res.Message = "Model was not found";
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
            return res;
        }

        public virtual Task<OperationResult<IQueryable<TModel>>> GetAllAsQueryable(bool asNoTracking = false)
        {
            var res = new OperationResult<IQueryable<TModel>>(_tableName);
            try
            {
                res.Model = asNoTracking ? _model.AsNoTracking() : _model.AsQueryable();
                res.Success = true;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
            return Task.FromResult(res);
        }

        public virtual Task<OperationResult<IQueryable<TModel>>> AllIncluding(
            Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>? include = null,
            bool asNoTracking = false)
        {
            var res = new OperationResult<IQueryable<TModel>>(_tableName);
            try
            {
                IQueryable<TModel> query = asNoTracking ? _model.AsNoTracking() : _model.AsQueryable();
                if (include is not null)
                    query = include(query);

                res.Model = query;
                res.Success = true;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
            return Task.FromResult(res);
        }

        public virtual IQueryable<TModel> Paginated(int pageSize, IQueryable<TModel> models, int pageIndex = 1, bool asNoTracking = false)
        {
            var query = asNoTracking ? models.AsNoTracking() : models;
            return query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public virtual async Task<OperationResult<TModel>> SaveAsync(CancellationToken cancellationToken = default)
        {
            var res = new OperationResult<TModel>(_tableName);
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                res.Success = true;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
            return res;
        }
    }
}
