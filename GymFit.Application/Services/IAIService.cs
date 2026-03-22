using GymFit.Application.Common;
using GymFit.Application.DTOs.AI;

namespace GymFit.Application.Services;

public interface IAIService
{
    Task<ServiceResult<AiPlanResponseDto>> GenerateWorkoutPlanAsync(
        Guid userId,
        string userInput,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AiPlanResponseDto>> GenerateDietPlanAsync(
        Guid userId,
        string userInput,
        CancellationToken cancellationToken = default);
}
