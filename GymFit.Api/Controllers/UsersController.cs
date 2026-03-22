using GymFit.Api.Authorization;
using GymFit.Api.Extensions;
using GymFit.Application.DTOs.Users;
using GymFit.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymFit.Api.Controllers;

[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.RequireUser)]
public sealed class UsersController : ApiV1ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileDto>> GetMe(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var profile = await _userService.GetProfileAsync(userId, cancellationToken);
        return Ok(profile);
    }

    [HttpPatch("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileDto>> PatchMe(
        [FromBody] UpdateUserProfileDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var profile = await _userService.UpdateProfileAsync(userId, userId, request, cancellationToken);
        return Ok(profile);
    }
}
