using IdentityService.Application.Common.Interfaces;
using IdentityService.Domain.Interfaces;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Persistence.Repositories;
using IdentityService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Infrastructure;

/// <summary>
/// Dependency injection configuration for Infrastructure layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database - Use cluster service name for PostgreSQL
        var dbHost = configuration["PostgreSQL:HostName"] ?? "postgresql-ha-pgpool.databases.svc.cluster.local";
        var dbPort = configuration["PostgreSQL:Port"] ?? "5432";
        var dbDatabase = configuration["PostgreSQL:Database"] ?? "appdb";
        var dbUsername = configuration["PostgreSQL:Username"] ?? "postgres";
        var dbPassword = configuration["PostgreSQL:Password"] ?? "";
        
        var connectionString = string.IsNullOrEmpty(dbPassword)
            ? $"Host={dbHost};Port={dbPort};Database={dbDatabase};Username={dbUsername};"
            : $"Host={dbHost};Port={dbPort};Database={dbDatabase};Username={dbUsername};Password={dbPassword};";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });
        });

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IMfaFactorRepository, MfaFactorRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IAuthorizationCodeRepository, AuthorizationCodeRepository>();
        services.AddScoped<IConsentRepository, ConsentRepository>();
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITotpService, TotpService>();
        services.AddHttpContextAccessor();

        // JWT Authentication
        var jwtSecretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "IdentityService";
        var jwtAudience = configuration["Jwt:Audience"] ?? "IdentityService";

        services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(jwtSecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        // Distributed Cache (Redis) - Use cluster service name
        var redisHost = configuration["Redis:HostName"] ?? "redis-cluster.middleware.svc.cluster.local";
        var redisPort = configuration["Redis:Port"] ?? "6379";
        var redisPassword = configuration["Redis:Password"] ?? "";
        
        if (!string.IsNullOrEmpty(redisHost))
        {
            var redisConnectionString = string.IsNullOrEmpty(redisPassword)
                ? $"{redisHost}:{redisPort}"
                : $"{redisHost}:{redisPort},password={redisPassword}";
                
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = configuration["Cache:InstanceName"] ?? "IdentityService";
            });
        }
        else
        {
            // Fallback to in-memory cache if Redis is not configured
            services.AddDistributedMemoryCache();
        }
        
        // RabbitMQ Event Bus
        services.AddSingleton<IEventBus, MessageBrokers.RabbitMQEventBus>();

        return services;
    }
}

