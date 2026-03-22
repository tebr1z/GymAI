using GymFit.Application.Common;

namespace GymFit.Application.Services;

public interface IAiQuotaService
{
    Task<ServiceResult> EnsureWithinMonthlyAiQuotaAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ServiceResult> RecordSuccessfulAiCallAsync(Guid userId, CancellationToken cancellationToken = default);
}
