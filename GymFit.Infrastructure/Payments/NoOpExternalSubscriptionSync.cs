using GymFit.Application.Abstractions.Payments;

namespace GymFit.Infrastructure.Payments;

/// <summary>Placeholder until Stripe/PayPal webhooks are wired. Replace registration in DI with a real implementation.</summary>
public sealed class NoOpExternalSubscriptionSync : IExternalSubscriptionSync
{
    public Task UpsertFromProviderAsync(ExternalSubscriptionSnapshot snapshot, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
