using GymFit.Domain.Enums;

namespace GymFit.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public SubscriptionTier Tier { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    /// <summary>Optional: e.g. Stripe, PayPal — for webhook reconciliation.</summary>
    public string? ExternalProvider { get; set; }

    public string? ExternalCustomerId { get; set; }
    public string? ExternalSubscriptionId { get; set; }
}
