using Asp.Versioning;
using GymFit.Api.Filters;
using GymFit.Api.Swagger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GymFit.Api.Extensions;

public static class ApiPresentationExtensions
{
    public static IServiceCollection AddGymFitApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new HeaderApiVersionReader("X-Api-Version"),
                new MediaTypeApiVersionReader(),
                new QueryStringApiVersionReader("api-version"));
        })
            .AddMvc()
            .AddApiExplorer(options => { options.GroupNameFormat = "'v'VVV"; });

        return services;
    }

    public static IServiceCollection AddGymFitControllersWithEnvelope(this IServiceCollection services)
    {
        services.AddControllers(options => { options.Filters.Add<ApiEnvelopeResultFilter>(); });
        return services;
    }

    public static IServiceCollection AddGymFitSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGenOptions>();
        return services;
    }
}
