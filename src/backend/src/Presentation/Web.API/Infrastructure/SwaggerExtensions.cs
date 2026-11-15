using Microsoft.OpenApi.Models;

namespace Web.API.Infrastructure;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Redarbor Product Inventory API",
                Version = "v1",
                Description = "RESTful API with Onion Architecture, CQRS (MediatR), EF Core Read / Dapper Write, and OAuth2/JWT PBAC."
            });
            var securityScheme = BuildBearerSecurityScheme();
            options.AddSecurityDefinition("Bearer", securityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() }
            });
        });

        return services;
    }

    private static OpenApiSecurityScheme BuildBearerSecurityScheme()
    {
        return new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter JWT Bearer token: Bearer {token}",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        };
    }
}
