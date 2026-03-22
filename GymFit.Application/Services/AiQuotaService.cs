using GymFit.Application.Abstractions;
using GymFit.Application.Common;
using GymFit.Application.Configuration;
using GymFit.Application.Repositories;
using GymFit.Domain.Entities;
using GymFit.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymFit.Application.Services;

public sealed class AiQuotaService : IAiQuotaService
{
    private readonly ISubscriptionRepository _subscriptions;
    private readonly IAiUsageRepository _usage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SubscriptionTierLimitsOptions _limits;
    private readonly ILogger<AiQuotaService> _logger;

    public AiQuotaService(
        ISubscriptionRepository subscriptions,
        IAiUsageRepository usage,
        IUnitOfWork unitOfWork,
        IOptions<SubscriptionTierLimitsOptions> limits,
        ILogger<AiQuotaService> logger)
    {
        _subscriptions = subscriptions;
        _usage = usage;
        _unitOfWork = unitOfWork;
        _limits = limits.Value;
        _logger = logger;
    }

    public Task<ServiceResult> EnsureWithinMonthlyAiQuotaAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        ServiceExecution.RunAsync(_logger, nameof(EnsureWithinMonthlyAiQuotaAsync), async () =>
        {
            if (userId == Guid.Empty)
                return ServiceResult.Fail("Invalid user id.", ServiceFailureKind.BadRequest);

            var now = DateTime.UtcNow;
            var active = await _subscriptions.GetActiveForUserAsync(userId, now, cancellationToken);
            var tier = active?.Tier ?? SubscriptionTier.Free;
            var limit = GetMonthlyLimit(tier);
            if (limit < 0)
                return ServiceResult.Ok();

            if (limit == 0)
            {
                return ServiceResult.Fail(
                    "AI features are not available on your current plan. Please upgrade to Pro or Premium.",
                    ServiceFailureKind.BadRequest);
            }

            var periodKey = BillingPeriod.CurrentUtcMonthKey();
            var used = await _usage.GetRequestCountAsync(userId, periodKey, cancellationToken);
            if (used >= limit)
            {
                return ServiceResult.Fail(
                    $"You have reached your monthly AI limit ({limit} requests) for the {tier} plan. Upgrade or wait until next month.",
                    ServiceFailureKind.BadRequest);
            }

            return ServiceResult.Ok();
        });

    public Task<ServiceResult> RecordSuccessfulAiCallAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        ServiceExecution.RunAsync(_logger, nameof(RecordSuccessfulAiCallAsync), async () =>
        {
            if (userId == Guid.Empty)
                return ServiceResult.Fail("Invalid user id.", ServiceFailureKind.BadRequest);

            var now = DateTime.UtcNow;
            var active = await _subscriptions.GetActiveForUserAsync(userId, now, cancellationToken);
            var tier = active?.Tier ?? SubscriptionTier.Free;
            var limit = GetMonthlyLimit(tier);
            if (limit < 0)
                return ServiceResult.Ok();

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
