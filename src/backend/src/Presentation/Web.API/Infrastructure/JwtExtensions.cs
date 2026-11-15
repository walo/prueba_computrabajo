using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Web.API.Infrastructure;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IConfiguration>((options, config) => ConfigureJwtBearerOptions(options, config));
        services.AddAuthorization();

        return services;
    }

    private static void ConfigureJwtBearerOptions(JwtBearerOptions options, IConfiguration config)
    {
        var secretKey = config["JWT_SECRET_KEY"]
            ?? config["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT_SECRET_KEY is not configured.");
        var issuer = config["JWT_ISSUER"] ?? config["Jwt:Issuer"] ?? "RedarborInventoryApi";
        var audience = config["JWT_AUDIENCE"] ?? config["Jwt:Audience"] ?? "RedarborInventoryClient";
        options.MapInboundClaims = false;
        options.TokenValidationParameters = BuildValidationParameters(secretKey, issuer, audience);
    }

    private static TokenValidationParameters BuildValidationParameters(string secretKey, string issuer, string audience)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    }
}
