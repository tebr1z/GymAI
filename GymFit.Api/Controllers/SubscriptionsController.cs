using GymFit.Api.Authorization;
using GymFit.Api.Extensions;
using GymFit.Application.DTOs.Subscriptions;
using GymFit.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymFit.Api.Controllers;

[Route("api/[controller]")]
public sealed class SubscriptionsController : ApiV1ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [Authorize(Policy = AuthorizationPolicies.RequireUser)]
    [HttpGet("me")]
    [ProducesResponseType(typeof(SubscriptionOverviewDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SubscriptionOverviewDto>> GetMine(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var result = await _subscriptionService.GetOverviewForUserAsync(userId, cancellationToken);
        return MapToActionResult(result);
    }

    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    [HttpPost("assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(
        [FromBody] AssignSubscriptionDto request,
        CancellationToken cancellationToken)
    {
        var result = await _subscriptionService.AssignAsync(request, cancellationToken);
        return MapToActionResult(result);
    }
}
