using BaseRepository.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseRepository.BaseInterface
{
    public class BaseRepository<TModel, TKey> : IBaseRepository<TModel, TKey> where TModel : class
    {
        private readonly YourContext _context;
        private readonly IHttpContextAccessor accessor;
        private readonly DbSet<TModel> _model;
        public BaseRepository(YourContext context, IHttpContextAccessor _accessor)
        {
            _context = context;
            accessor = _accessor;
            _model = _context.Set<TModel>();
        }
        public virtual async Task<OperationResult<TModel>> AddAsync(TModel model)
        {
            var res = new OperationResult<TModel>(_model.ToString());
            try
            {
                await _model.AddAsync(model);
                res.Success = true;
                res.Model = Model;
                return res;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                return res;
            }
        }
        public virtual OperationResult<TModel> Update(TModel model)
        {
            var res = new OperationResult(_model.ToString());
            try
            {
                _context.Set<TModel>().Attach(model);
                _context.Entry(model).State = EntityState.Modified;
                res.Success = true;
                rs.Model = model;
                return res;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                return res;
            }
        }
        public virtual async Task<OperationResult<TModel>> DeleteAsync(TModel model)
        {
            var res = new OperationResult(_model.ToString());
            try
            {
                _model.Remove(model);
                await SaveAsync();

                res.Success = true;
                res.Model = model;
                return res;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                return res;
            }
        }
        public virtual async Task<OperationResult<TModel>> DeleteAllAsync(IQueryable<TModel> deleteModels)
        {
            var res = new OperationResult(_model.ToString());
            try
            {
                _context.Set<TModel>().RemoveRange(deleteModels.AsEnumerable());
                await SaveAsync();
                res.Success = true;
                return res;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                return res;
            }
        }
        public virtual async Task<OperationResult<TModel>> DeleteAsync(TKey id)
        {
            var res = new OperationResult(_model.ToString());

            try
            {
                var model = await GetAsync(id);
                if (model != null)
                {
                    _model.Remove(model);
                    await SaveAsync();
                    res.Success = true;
                    res.Model = model;
                    return res;
                }
                else
                    res.Message = "Model not found";

            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
            return res;
        }
        public virtual async Task<OperationResult<TModel>> GetAsync(TKey id)
        {
            var res = new OperationResult(_model.ToString());
            try
            {
                var model = await _model.FindAsync(id);
                if (model != null)
                {
                    res.Success = true;
                    res.Model = model;
                }
                else
                {
                    res.Message = "Model was not Found";
                }
                return res;
            }
            catch (System.Exception ex)
            {
                res.Message = ex;
                return res;
            }
        }
        public IQueryable<TModel> GetAllAsQueryable(bool asNoTracking = false)
        {
            return asNoTracking ? _model.AsNoTracking().AsQueryable() : _model.AsQueryable();
        }
        public IQueryable<TModel> GetAllAsQueryable(Func<TModel, bool> predicate, bool asNoTracking = false)
        {
            return asNoTracking ? _model.AsNoTracking().AsEnumerable().Where(predicate).AsQueryable() : _model.Where(predicate).AsQueryable();
        }
        public IQueryable<TModel> AllIncluding(Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>> include = null, bool asNoTracking = false)
        {
            IQueryable<TModel> query = asNoTracking ? _model.AsNoTracking().AsQueryable() : _model.AsQueryable();
            if (include != null)
                query = include(query);

            return query;
        }
        public TModel GetInclude(Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>> include, Func<TModel, bool> predicate)
        {
            IQueryable<TModel> query = _model.AsQueryable();
            query = include(query);
            query = query.Where(predicate).AsQueryable();

            var res = query.FirstOrDefault();
            return res;
            //IQueryable<TModel> query = _model.AsQueryable();
            // query = include(query);
            //query = query.Where(predicate).AsQueryable()
            //var res2 = res.FindAsync(key);
            //return await (include(_model) as DbSet<TModel>).FindAsync(key);
            //return model;
        }
        public async Task<OperationResult> SaveAsync()
        {
            var res = new OperationResult(_model.ToString());
            try
            {
                await _context.SaveChangesAsync();
                res.Success = true;
                return res;
            }
            catch (Exception ex)
            {
                res.Success = false;
                res.Message = ex.Message.ToString();
                return res;
            }
        }

        public void Dispose()
        {
            // throw new NotImplementedException();
        }
        public IQueryable<TModel> Paginated(int pageSize, IQueryable<TModel> Models, int pageIndex = 1, bool asNoTracking = false)
        {
            return Models.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }
    }
}
