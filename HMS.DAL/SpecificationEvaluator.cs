using HMS.DAL.Contracts;
using HMS.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace HMS.DAL
{
    public static class SpecificationEvaluator
    {
        public static IQueryable<TEntity> CreateQuery<TEntity, TKey>(
    IQueryable<TEntity> inputQuery,
    ISpecifications<TEntity, TKey> specifications) where TEntity : BaseEntity<TKey>
        {
            var query = inputQuery;

            if (specifications.Criteria is not null)
                query = query.Where(specifications.Criteria);

            if (specifications.OrderBy is not null)
                query = query.OrderBy(specifications.OrderBy);

            if (specifications.OrderByDescending is not null)
                query = query.OrderByDescending(specifications.OrderByDescending);

            if (specifications.IncludeExpressions.Any())
                query = specifications.IncludeExpressions
                    .Aggregate(query, (current, include) => current.Include(include));
            if (specifications.IncludeStrings.Any())
                query = specifications.IncludeStrings
                    .Aggregate(query, (current, include) => current.Include(include));
            if (specifications.IsPaginated)
                query = query.Skip(specifications.Skip).Take(specifications.Take);

            return query;
        }
    }
}
