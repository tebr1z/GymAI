using GymFit.Application.Abstractions;
using GymFit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymFit.Infrastructure.Data;

/// <summary>
/// EF Core: lazy-loading proxies are not enabled (no <c>UseLazyLoadingProxies</c>). Navigations load only via <c>Include</c> or explicit loads.
/// Default tracking is <see cref="Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking"/> (see <c>AddDbContext</c>).
/// </summary>
public sealed class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>Application users (members, trainers, admins).</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>Trainer marketplace profiles (maps to &quot;trainers&quot; in the domain API).</summary>
    public DbSet<TrainerProfile> TrainerProfiles => Set<TrainerProfile>();

    /// <summary>Workout / diet plans owned by users.</summary>
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<AiUsageLedger> AiUsageLedgers => Set<AiUsageLedger>();
    public DbSet<TrainerOrder> TrainerOrders => Set<TrainerOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
