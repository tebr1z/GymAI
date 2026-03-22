using GymFit.Domain.Enums;

namespace GymFit.Application.DTOs.Subscriptions;

public sealed class SubscriptionOverviewDto
{
    public SubscriptionTier EffectiveTier { get; set; }
    public SubscriptionStatus? Status { get; set; }
    public DateTime? PeriodEndUtc { get; set; }
    public int AiRequestsLimitPerMonth { get; set; }
    public int AiRequestsUsedThisMonth { get; set; }
    public bool AiUnlimited { get; set; }
}
