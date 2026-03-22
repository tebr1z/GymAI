using GymFit.Application.Common;
using GymFit.Application.DTOs.Subscriptions;

namespace GymFit.Application.Services;

public interface ISubscriptionService
{
    Task<ServiceResult<SubscriptionOverviewDto>> GetOverviewForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> AssignAsync(AssignSubscriptionDto request, CancellationToken cancellationToken = default);
}
