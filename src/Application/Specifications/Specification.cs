using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BaseRepository.Application.Specifications;

/// <summary>
/// Base class for query specifications. Derive from this and use the protected builder
/// methods in your constructor to describe a query without depending on EF Core or any
/// other persistence technology.
/// </summary>
public abstract class Specification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public int Skip { get; private set; }
    public int Take { get; private set; }
    public bool IsPagingEnabled { get; private set; }
    public bool AsNoTracking { get; private set; } = true;

    protected Specification()
    {
    }

    protected Specification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    protected void AddCriteria(Expression<Func<T, bool>> criteria) => Criteria = criteria;

    protected void AddInclude(Expression<Func<T, object>> includeExpression) => Includes.Add(includeExpression);

    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression) => OrderBy = orderByExpression;

    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression) =>
        OrderByDescending = orderByDescendingExpression;

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    protected void ApplyTracking() => AsNoTracking = false;
}
