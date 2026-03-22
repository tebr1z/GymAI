using GymFit.Domain.Enums;

namespace GymFit.Application.Services;

public interface IAiQuotaService
{
    Task<SubscriptionTier> GetEffectiveTierAsync(Guid userId, CancellationToken cancellationToken = default);

    Task EnsureWithinMonthlyAiQuotaAsync(Guid userId, CancellationToken cancellationToken = default);

    Task RecordSuccessfulAiCallAsync(Guid userId, CancellationToken cancellationToken = default);
}
