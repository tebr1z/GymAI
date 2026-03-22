using AutoMapper;
using GymFit.Application.Abstractions;
using GymFit.Application.DTOs.Users;
using GymFit.Application.Repositories;

namespace GymFit.Application.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(IUserRepository users, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdWithTrainerProfileAsync(userId, cancellationToken);
        if (user is null)
            throw new KeyNotFoundException("User was not found.");

        return _mapper.Map<UserProfileDto>(user);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(
        Guid requestingUserId,
        Guid targetUserId,
        UpdateUserProfileDto request,
        CancellationToken cancellationToken = default)
    {
        if (requestingUserId != targetUserId)
            throw new InvalidOperationException("You can only update your own profile.");

        var user = await _users.GetByIdTrackedAsync(targetUserId, cancellationToken);
        if (user is null)
            throw new KeyNotFoundException("User was not found.");

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

        var refreshed = await _users.GetByIdWithTrainerProfileAsync(targetUserId, cancellationToken)
                        ?? throw new InvalidOperationException("Failed to load updated user.");
        return _mapper.Map<UserProfileDto>(refreshed);
    }
}
