using GymFit.Application.Mapping;
using GymFit.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GymFit.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITrainerService, TrainerService>();
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IAiQuotaService, AiQuotaService>();
        return services;
    }
}
