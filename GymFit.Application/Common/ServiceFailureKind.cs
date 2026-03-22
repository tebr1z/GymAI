namespace GymFit.Application.Common;

/// <summary>Maps to HTTP status codes for API translation.</summary>
public enum ServiceFailureKind
{
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    Conflict = 409,
    ServiceUnavailable = 503
}
