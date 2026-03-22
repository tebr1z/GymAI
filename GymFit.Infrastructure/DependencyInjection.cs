using System.Net.Http.Headers;
using GymFit.Application.Abstractions;
using GymFit.Application.Abstractions.Payments;
using GymFit.Application.Configuration;
using GymFit.Application.Repositories;
using GymFit.Application.Services;
using GymFit.Infrastructure.Data;
using GymFit.Infrastructure.Payments;
using GymFit.Infrastructure.Repositories;
using GymFit.Infrastructure.Security;
using GymFit.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GymFit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.Configure<OpenAiSettings>(configuration.GetSection(OpenAiSettings.SectionName));
        services.Configure<SubscriptionTierLimitsOptions>(
            configuration.GetSection(SubscriptionTierLimitsOptions.SectionName));

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddHttpClient("OpenAi", (sp, client) =>
        {
            var openAi = sp.GetRequiredService<IOptions<OpenAiSettings>>().Value;
            var baseUrl = openAi.BaseUrl.TrimEnd('/') + "/";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(Math.Max(1, openAi.TimeoutSeconds));
            if (!string.IsNullOrWhiteSpace(openAi.ApiKey))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAi.ApiKey);
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITrainerProfileRepository, TrainerProfileRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<ITrainerOrderRepository, TrainerOrderRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IAiUsageRepository, AiUsageRepository>();

        services.AddScoped<IAIService, OpenAiService>();
        services.AddScoped<IExternalSubscriptionSync, NoOpExternalSubscriptionSync>();

        return services;
    }
}
