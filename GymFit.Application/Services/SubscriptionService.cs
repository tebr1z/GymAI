using GymFit.Application.Abstractions;
using GymFit.Application.Common;
using GymFit.Application.Configuration;
using GymFit.Application.DTOs.Subscriptions;
using GymFit.Application.Repositories;
using GymFit.Domain.Entities;
using GymFit.Domain.Enums;
using Microsoft.Extensions.Options;

namespace GymFit.Application.Services;

public sealed class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptions;
    private readonly IAiUsageRepository _usage;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SubscriptionTierLimitsOptions _limits;

    public SubscriptionService(
        ISubscriptionRepository subscriptions,
        IAiUsageRepository usage,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IOptions<SubscriptionTierLimitsOptions> limits)
    {
        _subscriptions = subscriptions;
        _usage = usage;
        _users = users;
        _unitOfWork = unitOfWork;
        _limits = limits.Value;
    }

    public async Task<SubscriptionOverviewDto> GetOverviewForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var active = await _subscriptions.GetActiveForUserAsync(userId, now, cancellationToken);
        var tier = active?.Tier ?? SubscriptionTier.Free;
        var periodKey = BillingPeriod.CurrentUtcMonthKey(now);
        var used = await _usage.GetRequestCountAsync(userId, periodKey, cancellationToken);
        var limit = GetMonthlyLimit(tier);

        return new SubscriptionOverviewDto
        {
            EffectiveTier = tier,
            Status = active?.Status,
            PeriodEndUtc = active?.EndDate,
            AiRequestsLimitPerMonth = limit,
            AiRequestsUsedThisMonth = used,
            AiUnlimited = limit < 0
        };
    }

    public async Task AssignAsync(AssignSubscriptionDto request, CancellationToken cancellationToken = default)
    {
        if (!await _users.ExistsAsync(request.UserId, cancellationToken))
            throw new KeyNotFoundException("User was not found.");

        var now = DateTime.UtcNow;
        if (request.EndDateUtc <= now)
            throw new InvalidOperationException("Subscription end date must be in the future.");

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
    }

    private int GetMonthlyLimit(SubscriptionTier tier) =>
        tier switch
        {
            SubscriptionTier.Free => _limits.FreeAiRequestsPerMonth,
            SubscriptionTier.Pro => _limits.ProAiRequestsPerMonth,
            SubscriptionTier.Premium => _limits.PremiumAiRequestsPerMonth,
            _ => _limits.FreeAiRequestsPerMonth
        };
}
