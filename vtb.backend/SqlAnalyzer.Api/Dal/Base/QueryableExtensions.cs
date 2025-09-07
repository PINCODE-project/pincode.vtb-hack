namespace SqlAnalyzer.Api.Dal.Base;

public static class QueryableExtensions
{
    public static IQueryable<T> UseLimiter<T>(this IQueryable<T> q, int? skip, int? take)
        where T : class
    {
        if (skip.HasValue)
        {
            q = q.Skip(skip.Value);
        }

        if (take.HasValue)
        {
            q = q.Take(take.Value);
        }

        return q;
    }
}