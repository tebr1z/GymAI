using GymFit.Application.Common;
using GymFit.Application.DTOs.Common;
using Microsoft.EntityFrameworkCore;

namespace GymFit.Infrastructure.Extensions;

public static class QueryablePaginationExtensions
{
    /// <summary>Executes count + page slice in two round-trips. Use inside repositories/services on EF queries.</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> source,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default,
        int maxPageSize = Pagination.MaxPageSize)
    {
        var (p, ps) = Pagination.Normalize(page, pageSize, maxPageSize);
        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip(Pagination.Skip(p, ps))
            .Take(ps)
            .ToListAsync(cancellationToken);
        return PagedResult<T>.Create(items, total, p, ps);
    }
}
