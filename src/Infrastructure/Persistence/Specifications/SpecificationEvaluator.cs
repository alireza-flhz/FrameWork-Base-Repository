using System.Linq;
using BaseRepository.Application.Specifications;
using Microsoft.EntityFrameworkCore;

namespace BaseRepository.Infrastructure.Persistence.Specifications;

public static class SpecificationEvaluator<T> where T : class
{
    public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification, bool evaluatePaging = true)
    {
        var query = inputQuery;

        if (specification.Criteria is not null)
            query = query.Where(specification.Criteria);

        query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

        if (specification.OrderBy is not null)
            query = query.OrderBy(specification.OrderBy);
        else if (specification.OrderByDescending is not null)
            query = query.OrderByDescending(specification.OrderByDescending);

        if (evaluatePaging && specification.IsPagingEnabled)
            query = query.Skip(specification.Skip).Take(specification.Take);

        if (specification.AsNoTracking)
            query = query.AsNoTracking();

        return query;
    }
}
