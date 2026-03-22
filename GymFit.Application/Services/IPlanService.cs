using GymFit.Application.Common;
using GymFit.Application.DTOs.Common;
using GymFit.Application.DTOs.Plans;

namespace GymFit.Application.Services;

public interface IPlanService
{
    Task<ServiceResult<PlanDto>> CreateAsync(Guid userId, CreatePlanDto request, CancellationToken cancellationToken = default);

    Task<ServiceResult<PagedResult<PlanDto>>> ListForUserAsync(
        Guid userId,
        PaginationQuery query,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<PlanDto>> GetAsync(Guid planId, Guid requesterUserId, CancellationToken cancellationToken = default);
}
