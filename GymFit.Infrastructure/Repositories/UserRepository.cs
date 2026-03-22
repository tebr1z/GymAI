using GymFit.Application.Repositories;
using GymFit.Domain.Entities;
using GymFit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymFit.Infrastructure.Repositories;

public sealed class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await DbSet.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == normalized, cancellationToken);
    }

    public async Task<User?> GetByIdWithTrainerProfileAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(x => x.TrainerProfile)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
