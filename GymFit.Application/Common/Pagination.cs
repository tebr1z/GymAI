namespace GymFit.Application.Common;

/// <summary>Shared pagination bounds and normalization for queries and repositories.</summary>
public static class Pagination
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    /// <summary>Returns a valid page (≥1) and page size in [1, <paramref name="maxPageSize"/>].</summary>
    public static (int Page, int PageSize) Normalize(int page, int pageSize, int maxPageSize = MaxPageSize)
    {
        var p = Math.Max(DefaultPage, page);
        var ps = Math.Clamp(pageSize, 1, maxPageSize);
        return (p, ps);
    }

    public static int Skip(int page, int pageSize) => (Math.Max(1, page) - 1) * pageSize;
}
