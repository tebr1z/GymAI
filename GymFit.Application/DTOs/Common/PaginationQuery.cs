using GymFit.Application.Common;

namespace GymFit.Application.DTOs.Common;

/// <summary>Reusable page parameters for list endpoints.</summary>
public sealed class PaginationQuery
{
    public int Page { get; set; } = Pagination.DefaultPage;
    public int PageSize { get; set; } = Pagination.DefaultPageSize;
}
