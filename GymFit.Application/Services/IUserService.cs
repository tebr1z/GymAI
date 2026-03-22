using GymFit.Application.DTOs.Users;

namespace GymFit.Application.Services;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<UserProfileDto> UpdateProfileAsync(
        Guid requestingUserId,
        Guid targetUserId,
        UpdateUserProfileDto request,
        CancellationToken cancellationToken = default);
}
