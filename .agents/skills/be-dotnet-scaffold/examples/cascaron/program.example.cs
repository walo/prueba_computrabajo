using System.Text;
using Application;
using Application.Common.Interfaces;
using Infrastructure.Authentication;
using Infrastructure.ExternalServices;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Seeding;
using Infrastructure.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Web.API.HealthChecks;
using Web.API.Infrastructure;
using Web.API.Middlewares;
using Web.API.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Register decoupled Onion layers (Core + 4 Infrastructure Class Libraries)
builder.Services.AddApplication();
builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddPersistenceInfrastructure(builder.Configuration);
builder.Services.AddAuthenticationInfrastructure();
builder.Services.AddExternalServicesInfrastructure(builder.Configuration);

// 2. Presentation & Context Services
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();

// 3. Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

// 4. Ultralight JWT Authentication
var secretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// 5. Dynamic CORS
const string corsPolicyName = "AllowFrontend";
builder.Services.AddCorsInfrastructure(builder.Configuration, corsPolicyName);

var app = builder.Build();

// 6. Database Migration & Initial Seeding
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeding");
    await InitialDataSeeder.SeedAsync(dbContext, passwordHasher, logger);
}

// 7. Strict Security Middleware Pipeline
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseCors(corsPolicyName);
app.UseAuthentication();
app.UseMiddleware<JwtBlacklistMiddleware>();
app.UseAuthorization();
app.UseMiddleware<PermissionAuthorizationMiddleware>();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapHealthChecks("/health/ready").AllowAnonymous();
app.MapControllers();

app.Run();

public partial class Program;
