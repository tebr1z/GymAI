using GymFit.Application.DTOs.AI;

namespace GymFit.Application.Services;

public interface IAIService
{
    Task<AiPlanResponseDto> GenerateWorkoutPlanAsync(Guid userId, string userInput, CancellationToken cancellationToken = default);

    Task<AiPlanResponseDto> GenerateDietPlanAsync(Guid userId, string userInput, CancellationToken cancellationToken = default);
}
