using Domain.Models.Entities;

namespace Infra.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> WhereActive<T>(this IQueryable<T> query) where T : BaseEntity
        {
            return query.Where(x => x.IsActive);
        }
    }
}
