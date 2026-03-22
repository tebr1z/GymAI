namespace GymFit.Application.Configuration;

/// <summary>Monthly AI caps per subscription tier. Use -1 for unlimited.</summary>
public sealed class SubscriptionTierLimitsOptions
{
    public const string SectionName = "SubscriptionTierLimits";

    public int FreeAiRequestsPerMonth { get; set; } = 10;
    public int ProAiRequestsPerMonth { get; set; } = 200;
    public int PremiumAiRequestsPerMonth { get; set; } = -1;
}
