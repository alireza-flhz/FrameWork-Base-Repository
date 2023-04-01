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
        public Task<OperationResult<IQueryable<TModel>>> GetAllAsQueryable(bool asNoTracking = false)
        {

            var res = new OperationResult(_model.ToString());
            try
            {

                if (asNoTracking)
                {
                    res.Model = _model.AsNoTracking().AsQueryable();
                    res.Success = true;

                }
                else
                {
                    res.Model = _model.AsQueryable();
                    res.Success = true;
                }
                return res;

            }
            catch (System.Exception ex)
            {
                res.Message = ex;
                return res;
            }
        }
        public Task<OperationResult<IQueryable<TModel>>> AllIncluding(Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>> include = null, bool asNoTracking = false)
        {

            var res = new OperationResult(_model.ToString());
            try
            {
                IQueryable<TModel> query = asNoTracking ? _model.AsNoTracking().AsQueryable() : _model.AsQueryable();
                if (include != null)
                    query = include(query);
                res.Model = query;
                res.Success = true;
                return res;

            }
            catch (System.Exception ex)
            {
                res.Message = ex;
                return res;
            }

        }
        public async Task<OperationResult<TModel>> SaveAsync()
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
