using GymFit.Application.Abstractions;
using GymFit.Application.Common;
using GymFit.Application.Configuration;
using GymFit.Application.DTOs.Subscriptions;
using GymFit.Application.Repositories;
using GymFit.Domain.Entities;
using GymFit.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymFit.Application.Services;

public sealed class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptions;
    private readonly IAiUsageRepository _usage;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SubscriptionTierLimitsOptions _limits;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        ISubscriptionRepository subscriptions,
        IAiUsageRepository usage,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IOptions<SubscriptionTierLimitsOptions> limits,
        ILogger<SubscriptionService> logger)
    {
        _subscriptions = subscriptions;
        _usage = usage;
        _users = users;
        _unitOfWork = unitOfWork;
        _limits = limits.Value;
        _logger = logger;
    }

    public Task<ServiceResult<SubscriptionOverviewDto>> GetOverviewForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        ServiceExecution.RunAsync(_logger, nameof(GetOverviewForUserAsync), async () =>
        {
            if (userId == Guid.Empty)
                return ServiceResult<SubscriptionOverviewDto>.Fail("Invalid user id.", ServiceFailureKind.BadRequest);

            var now = DateTime.UtcNow;
            var active = await _subscriptions.GetActiveForUserAsync(userId, now, cancellationToken);
            var tier = active?.Tier ?? SubscriptionTier.Free;
            var periodKey = BillingPeriod.CurrentUtcMonthKey(now);
            var used = await _usage.GetRequestCountAsync(userId, periodKey, cancellationToken);
            var limit = GetMonthlyLimit(tier);

            return ServiceResult<SubscriptionOverviewDto>.Ok(new SubscriptionOverviewDto
            {
                EffectiveTier = tier,
                Status = active?.Status,
                PeriodEndUtc = active?.EndDate,
                AiRequestsLimitPerMonth = limit,
                AiRequestsUsedThisMonth = used,
                AiUnlimited = limit < 0
            });
        });

    public Task<ServiceResult> AssignAsync(AssignSubscriptionDto request, CancellationToken cancellationToken = default) =>
        ServiceExecution.RunAsync(_logger, nameof(AssignAsync), async () =>
        {
            if (request is null)
                return ServiceResult.Fail("Request body is required.", ServiceFailureKind.BadRequest);

            if (request.UserId == Guid.Empty)
                return ServiceResult.Fail("User id is invalid.", ServiceFailureKind.BadRequest);

            if (!await _users.ExistsAsync(request.UserId, cancellationToken))
                return ServiceResult.Fail("User was not found.", ServiceFailureKind.NotFound);

            var now = DateTime.UtcNow;
            if (request.EndDateUtc <= now)
            {
                return ServiceResult.Fail(
                    "Subscription end date must be in the future.",
                    ServiceFailureKind.BadRequest);
            }

            var trackedActives = await _subscriptions.ListActiveTrackedForUserAsync(request.UserId, now, cancellationToken);
            foreach (var existing in trackedActives)
            {
                existing.Status = SubscriptionStatus.Cancelled;
                _subscriptions.Update(existing);
            }

            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Tier = request.Tier,
                Status = SubscriptionStatus.Active,
                StartDate = now,
                EndDate = request.EndDateUtc,
                ExternalProvider = request.ExternalProvider,
                ExternalCustomerId = request.ExternalCustomerId,
                ExternalSubscriptionId = request.ExternalSubscriptionId
            };

            await _subscriptions.AddAsync(subscription, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult.Ok();
        });

    private int GetMonthlyLimit(SubscriptionTier tier) =>
        tier switch
        {
            SubscriptionTier.Free => _limits.FreeAiRequestsPerMonth,
            SubscriptionTier.Pro => _limits.ProAiRequestsPerMonth,
            SubscriptionTier.Premium => _limits.PremiumAiRequestsPerMonth,
            _ => _limits.FreeAiRequestsPerMonth
        };
}
