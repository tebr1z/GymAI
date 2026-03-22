using GymFit.Domain.Enums;

namespace GymFit.Application.DTOs.Subscriptions;

public sealed class AssignSubscriptionDto
{
    public Guid UserId { get; set; }
    public SubscriptionTier Tier { get; set; }
    public DateTime EndDateUtc { get; set; }

    public string? ExternalProvider { get; set; }
    public string? ExternalCustomerId { get; set; }
    public string? ExternalSubscriptionId { get; set; }
}
