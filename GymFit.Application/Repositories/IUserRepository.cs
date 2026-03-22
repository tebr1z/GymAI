using GymFit.Domain.Entities;

namespace GymFit.Application.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetByIdWithTrainerProfileAsync(Guid id, CancellationToken cancellationToken = default);
}
