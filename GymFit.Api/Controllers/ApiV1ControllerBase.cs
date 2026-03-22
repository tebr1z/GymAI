using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace GymFit.Api.Controllers;

/// <summary>Default API surface version (1.0). Add v2+ by introducing <c>ApiV2ControllerBase</c> and new routes when needed.</summary>
[ApiController]
[ApiVersion("1.0")]
public abstract class ApiV1ControllerBase : ControllerBase
{
}
