using GymFit.Application.Common;
using GymFit.Application.DTOs.Auth;

namespace GymFit.Application.Services;

public interface IAuthService
{
    Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default);

    Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto request, CancellationToken cancellationToken = default);
}
