using GymFit.Api.Authorization;
using GymFit.Api.Extensions;
using GymFit.Application.DTOs.Common;
using GymFit.Application.DTOs.Trainers;
using GymFit.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymFit.Api.Controllers;

/// <summary>Trainer marketplace: discovery, profile, and bookings.</summary>
[Route("api/trainers")]
public sealed class TrainerController : ApiV1ControllerBase
{
    private readonly ITrainerService _trainerService;

    public TrainerController(ITrainerService trainerService)
    {
        _trainerService = trainerService;
    }

    /// <summary>List approved trainers with optional filters and pagination.</summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TrainerSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TrainerSummaryDto>>> GetMarketplace(
        [FromQuery] TrainerMarketplaceQuery query,
        CancellationToken cancellationToken)
    {
        var page = await _trainerService.ListMarketplaceAsync(query, cancellationToken);
        return Ok(page);
    }

    /// <summary>Get a single trainer's public profile (marketplace-visible only if approved).</summary>
    [AllowAnonymous]
    [HttpGet("{trainerProfileId:guid}")]
    [ProducesResponseType(typeof(TrainerDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TrainerDetailDto>> GetTrainer(
        Guid trainerProfileId,
        CancellationToken cancellationToken)
    {
        var trainer = await _trainerService.GetTrainerAsync(trainerProfileId, cancellationToken);
        return Ok(trainer);
    }

    /// <summary>Create a booking; price is snapshotted from the trainer profile at booking time.</summary>
    [Authorize(Policy = AuthorizationPolicies.RequireUser)]
    [HttpPost("bookings")]
    [ProducesResponseType(typeof(TrainerOrderDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TrainerOrderDto>> BookTrainer(
        [FromBody] BookTrainerDto request,
        CancellationToken cancellationToken)
    {
        var clientId = User.GetRequiredUserId();
        var order = await _trainerService.BookTrainerAsync(clientId, request, cancellationToken);
        return Ok(order);
    }

    [Authorize(Policy = AuthorizationPolicies.RequireUser)]
    [HttpGet("bookings/me")]
    [ProducesResponseType(typeof(PagedResult<TrainerOrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TrainerOrderDto>>> MyBookings(
        [FromQuery] PaginationQuery query,
        CancellationToken cancellationToken)
    {
        var clientId = User.GetRequiredUserId();
        var page = await _trainerService.ListMyBookingsAsync(clientId, query, cancellationToken);
        return Ok(page);
    }

    [Authorize(Policy = AuthorizationPolicies.RequireTrainer)]
    [HttpGet("orders/me")]
    [ProducesResponseType(typeof(PagedResult<TrainerOrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TrainerOrderDto>>> MyTrainerOrders(
        [FromQuery] PaginationQuery query,
        CancellationToken cancellationToken)
    {
        var trainerId = User.GetRequiredUserId();
        var page = await _trainerService.ListTrainerOrdersAsync(trainerId, query, cancellationToken);
        return Ok(page);
    }
}
