using GymFit.Application.Abstractions;
using GymFit.Application.Common;
using GymFit.Application.Configuration;
using GymFit.Application.Repositories;
using GymFit.Domain.Entities;
using GymFit.Domain.Enums;
using Microsoft.Extensions.Options;

namespace GymFit.Application.Services;

public sealed class AiQuotaService : IAiQuotaService
{
    private readonly ISubscriptionRepository _subscriptions;
    private readonly IAiUsageRepository _usage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SubscriptionTierLimitsOptions _limits;

    public AiQuotaService(
        ISubscriptionRepository subscriptions,
        IAiUsageRepository usage,
        IUnitOfWork unitOfWork,
        IOptions<SubscriptionTierLimitsOptions> limits)
    {
        _subscriptions = subscriptions;
        _usage = usage;
        _unitOfWork = unitOfWork;
        _limits = limits.Value;
    }

    public async Task<SubscriptionTier> GetEffectiveTierAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var active = await _subscriptions.GetActiveForUserAsync(userId, now, cancellationToken);
        return active?.Tier ?? SubscriptionTier.Free;
    }

    public async Task EnsureWithinMonthlyAiQuotaAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tier = await GetEffectiveTierAsync(userId, cancellationToken);
        var limit = GetMonthlyLimit(tier);
        if (limit < 0)
            return;

        if (limit == 0)
        {
            throw new InvalidOperationException(
                "AI features are not available on your current plan. Please upgrade to Pro or Premium.");
        }

        var periodKey = BillingPeriod.CurrentUtcMonthKey();
        var used = await _usage.GetRequestCountAsync(userId, periodKey, cancellationToken);
        if (used >= limit)
        {
            throw new InvalidOperationException(
                $"You have reached your monthly AI limit ({limit} requests) for the {tier} plan. Upgrade or wait until next month.");
        }
    }

    public async Task RecordSuccessfulAiCallAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tier = await GetEffectiveTierAsync(userId, cancellationToken);
        var limit = GetMonthlyLimit(tier);
        if (limit < 0)
            return;

        var periodKey = BillingPeriod.CurrentUtcMonthKey();
        var ledger = await _usage.GetTrackedLedgerAsync(userId, periodKey, cancellationToken);
        if (ledger is null)
        {
            ledger = new AiUsageLedger
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PeriodKey = periodKey,
                RequestCount = 1
            };
            await _usage.AddAsync(ledger, cancellationToken);
        }
        else
        {
            ledger.RequestCount++;
            _usage.Update(ledger);
        }

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
