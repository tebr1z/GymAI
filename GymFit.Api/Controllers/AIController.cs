using GymFit.Api.Authorization;
using GymFit.Api.Extensions;
using GymFit.Application.DTOs.AI;
using GymFit.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymFit.Api.Controllers;

[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.RequireUser)]
public sealed class AIController : ApiV1ControllerBase
{
    private readonly IAIService _aiService;

    public AIController(IAIService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("workout-plan")]
    [ProducesResponseType(typeof(AiPlanResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AiPlanResponseDto>> WorkoutPlan(
        [FromBody] AiPlanRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var result = await _aiService.GenerateWorkoutPlanAsync(userId, request.UserInput, cancellationToken);
        return Ok(result);
    }

    [HttpPost("diet-plan")]
    [ProducesResponseType(typeof(AiPlanResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AiPlanResponseDto>> DietPlan(
        [FromBody] AiPlanRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var result = await _aiService.GenerateDietPlanAsync(userId, request.UserInput, cancellationToken);
        return Ok(result);
    }
}
