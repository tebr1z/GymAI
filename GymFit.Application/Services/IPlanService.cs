using GymFit.Application.DTOs.Common;
using GymFit.Application.DTOs.Plans;

namespace GymFit.Application.Services;

public interface IPlanService
{
    Task<PlanDto> CreateAsync(Guid userId, CreatePlanDto request, CancellationToken cancellationToken = default);

    Task<PagedResult<PlanDto>> ListForUserAsync(Guid userId, PaginationQuery query, CancellationToken cancellationToken = default);

    Task<PlanDto> GetAsync(Guid planId, Guid requesterUserId, CancellationToken cancellationToken = default);
}
