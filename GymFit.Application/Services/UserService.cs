using AutoMapper;
using GymFit.Application.Abstractions;
using GymFit.Application.Common;
using GymFit.Application.DTOs.Users;
using GymFit.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace GymFit.Application.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public Task<ServiceResult<UserProfileDto>> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default) =>
        ServiceExecution.RunAsync(_logger, nameof(GetProfileAsync), async () =>
        {
            if (userId == Guid.Empty)
                return ServiceResult<UserProfileDto>.Fail("Invalid user id.", ServiceFailureKind.BadRequest);

            var user = await _users.GetByIdWithTrainerProfileAsync(userId, cancellationToken);
            if (user is null)
                return ServiceResult<UserProfileDto>.Fail("User was not found.", ServiceFailureKind.NotFound);

            return ServiceResult<UserProfileDto>.Ok(_mapper.Map<UserProfileDto>(user));
        });

    public Task<ServiceResult<UserProfileDto>> UpdateProfileAsync(
        Guid requestingUserId,
        Guid targetUserId,
        UpdateUserProfileDto request,
        CancellationToken cancellationToken = default) =>
        ServiceExecution.RunAsync(_logger, nameof(UpdateProfileAsync), async () =>
        {
            if (request is null)
                return ServiceResult<UserProfileDto>.Fail("Request body is required.", ServiceFailureKind.BadRequest);

            if (requestingUserId == Guid.Empty || targetUserId == Guid.Empty)
                return ServiceResult<UserProfileDto>.Fail("Invalid user id.", ServiceFailureKind.BadRequest);

            if (requestingUserId != targetUserId)
            {
                return ServiceResult<UserProfileDto>.Fail(
                    "You can only update your own profile.",
                    ServiceFailureKind.Forbidden);
            }

            var user = await _users.GetByIdTrackedAsync(targetUserId, cancellationToken);
            if (user is null)
                return ServiceResult<UserProfileDto>.Fail("User was not found.", ServiceFailureKind.NotFound);

            if (!string.IsNullOrWhiteSpace(request.FullName))
                user.FullName = request.FullName.Trim();

            if (request.Weight.HasValue)
                user.Weight = request.Weight;

            if (request.Height.HasValue)
                user.Height = request.Height;

            if (request.Goal is not null)
                user.Goal = string.IsNullOrWhiteSpace(request.Goal) ? null : request.Goal.Trim();

            _users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var refreshed = await _users.GetByIdWithTrainerProfileAsync(targetUserId, cancellationToken);
            if (refreshed is null)
            {
                return ServiceResult<UserProfileDto>.Fail(
                    "Profile was updated but could not be reloaded.",
                    ServiceFailureKind.BadRequest);
            }

            return ServiceResult<UserProfileDto>.Ok(_mapper.Map<UserProfileDto>(refreshed));
        });
}
