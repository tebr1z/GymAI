using GymFit.Domain.Enums;

namespace GymFit.Application.Abstractions.Payments;

/// <summary>
/// Implement when integrating Stripe/PayPal/etc. Webhooks call <see cref="UpsertFromProviderAsync"/>
/// to keep subscriptions in sync with the payment provider.
/// </summary>
public interface IExternalSubscriptionSync
{
    Task UpsertFromProviderAsync(ExternalSubscriptionSnapshot snapshot, CancellationToken cancellationToken = default);
}

/// <summary>Normalized payload from a billing provider webhook or API.</summary>
public sealed record ExternalSubscriptionSnapshot(
    string Provider,
    string CustomerId,
    string SubscriptionId,
    Guid UserId,
    SubscriptionTier Tier,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    SubscriptionStatus Status);
