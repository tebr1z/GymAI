using GymFit.Application.Common;
using GymFit.Application.DTOs.Users;

namespace GymFit.Application.Services;

public interface IUserService
{
    Task<ServiceResult<UserProfileDto>> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ServiceResult<UserProfileDto>> UpdateProfileAsync(
        Guid requestingUserId,
        Guid targetUserId,
        UpdateUserProfileDto request,
        CancellationToken cancellationToken = default);
}
