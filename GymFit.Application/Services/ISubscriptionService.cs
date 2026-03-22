using GymFit.Application.DTOs.Subscriptions;

namespace GymFit.Application.Services;

public interface ISubscriptionService
{
    Task<SubscriptionOverviewDto> GetOverviewForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AssignAsync(AssignSubscriptionDto request, CancellationToken cancellationToken = default);
}
