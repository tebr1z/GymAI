using GymFit.Api.Authorization;
using GymFit.Api.Extensions;
using GymFit.Application.DTOs.Common;
using GymFit.Application.DTOs.Plans;
using GymFit.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymFit.Api.Controllers;

[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.RequireUser)]
public sealed class PlansController : ApiV1ControllerBase
{
    private readonly IPlanService _planService;

    public PlansController(IPlanService planService)
    {
        _planService = planService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(PlanDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PlanDto>> Create(
        [FromBody] CreatePlanDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var plan = await _planService.CreateAsync(userId, request, cancellationToken);
        return Ok(plan);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(PagedResult<PlanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PlanDto>>> ListMine(
        [FromQuery] PaginationQuery query,
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var plans = await _planService.ListForUserAsync(userId, query, cancellationToken);
        return Ok(plans);
    }

    [HttpGet("{planId:guid}")]
    [ProducesResponseType(typeof(PlanDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PlanDto>> Get(Guid planId, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var plan = await _planService.GetAsync(planId, userId, cancellationToken);
        return Ok(plan);
    }
}
