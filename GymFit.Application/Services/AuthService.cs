using AutoMapper;
using GymFit.Application.Abstractions;
using GymFit.Application.Common;
using GymFit.Application.DTOs.Auth;
using GymFit.Application.DTOs.Users;
using GymFit.Application.Repositories;
using GymFit.Domain.Entities;
using GymFit.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace GymFit.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public Task<ServiceResult<AuthResponseDto>> RegisterAsync(
        RegisterDto request,
        CancellationToken cancellationToken = default) =>
        ServiceExecution.RunAsync(_logger, nameof(RegisterAsync), async () =>
        {
            if (request is null)
                return ServiceResult<AuthResponseDto>.Fail("Request body is required.", ServiceFailureKind.BadRequest);

            var email = NormalizeEmail(request.Email);
            if (await _users.GetByEmailAsync(email, cancellationToken) is not null)
            {
                return ServiceResult<AuthResponseDto>.Fail(
                    "This email is already registered.",
                    ServiceFailureKind.Conflict);
            }

            var now = DateTime.UtcNow;
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName.Trim(),
                Email = email,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = UserRole.User,
                CreatedAt = now
            };

            await _users.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<AuthResponseDto>.Ok(BuildAuthResponse(user));
        });

    public Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto request, CancellationToken cancellationToken = default) =>
        ServiceExecution.RunAsync(_logger, nameof(LoginAsync), async () =>
        {
            if (request is null)
                return ServiceResult<AuthResponseDto>.Fail("Request body is required.", ServiceFailureKind.BadRequest);

            var email = NormalizeEmail(request.Email);
            var user = await _users.GetByEmailAsync(email, cancellationToken);
            if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                return ServiceResult<AuthResponseDto>.Fail(
                    "Invalid email or password.",
                    ServiceFailureKind.Unauthorized);
            }

            return ServiceResult<AuthResponseDto>.Ok(BuildAuthResponse(user));
        });

    private AuthResponseDto BuildAuthResponse(User user)
    {
        var token = _jwtTokenGenerator.CreateAccessToken(user.Id, user.Email, user.FullName, user.Role);
        return new AuthResponseDto
        {
            TokenType = "Bearer",
            AccessToken = token.Token,
            ExpiresAtUtc = token.ExpiresAtUtc,
            User = _mapper.Map<UserDto>(user)
        };
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
