using IdentityService.API.Filters;
using IdentityService.API.Middleware;
using IdentityService.Application;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.BackgroundJobs;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter.Prometheus.AspNetCore;
using Serilog;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .WriteTo.Console()
        .WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341");
});

// CORS - Environment-specific
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? new[] { "*" };
        
        if (allowedOrigins.Contains("*"))
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

// OpenTelemetry for Prometheus metrics
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation(options =>
            {
                options.Filter = httpContext =>
                {
                    // Filter out health check endpoints from metrics
                    var path = httpContext.Request.Path.Value?.ToLowerInvariant();
                    return !path?.StartsWith("/health") ?? true;
                });
                options.RecordException = true;
            })
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddPrometheusExporter();
    })
    .ConfigureResource(resource =>
    {
        resource.AddService(
            serviceName: "identity-service",
            serviceVersion: "1.0.0",
            serviceNamespace: "microservices");
    });

// Health Checks - Required for K8S
var healthChecksBuilder = builder.Services.AddHealthChecks()
    .AddDbContextCheck<Infrastructure.Persistence.ApplicationDbContext>("database", tags: new[] { "ready" });

// Add Redis health check
var redisHost = builder.Configuration["Redis:HostName"] ?? "redis-cluster.middleware.svc.cluster.local";
var redisPort = builder.Configuration["Redis:Port"] ?? "6379";
var redisPassword = builder.Configuration["Redis:Password"] ?? "";
if (!string.IsNullOrEmpty(redisHost))
{
    var redisConnectionString = string.IsNullOrEmpty(redisPassword)
        ? $"{redisHost}:{redisPort}"
        : $"{redisHost}:{redisPort},password={redisPassword}";
    
    healthChecksBuilder.AddRedis(redisConnectionString, "redis", tags: new[] { "ready" });
}

// Add RabbitMQ health check
var rabbitmqHost = builder.Configuration["RabbitMQ:HostName"] ?? "rabbitmq-cluster.middleware.svc.cluster.local";
var rabbitmqPort = int.Parse(builder.Configuration["RabbitMQ:Port"] ?? "5672");
var rabbitmqUsername = builder.Configuration["RabbitMQ:UserName"] ?? "admin";
var rabbitmqPassword = builder.Configuration["RabbitMQ:Password"] ?? "";
if (!string.IsNullOrEmpty(rabbitmqHost))
{
    healthChecksBuilder.AddRabbitMQ(
        rabbitConnectionString: $"amqp://{rabbitmqUsername}:{rabbitmqPassword}@{rabbitmqHost}:{rabbitmqPort}/",
        name: "rabbitmq",
        tags: new[] { "ready" });
}

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader("version"),
        new HeaderApiVersionReader("X-Version"));
});

// Application layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();

// Background jobs
builder.Services.AddHostedService<OutboxProcessorJob>();

// Controllers
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
    options.ReturnHttpNotAcceptable = true;
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Identity Service API",
        Version = "v1",
        Description = "Central authentication and identity management service for all microservices and external clients",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "API Support",
            Email = "support@example.com"
        }
    });

    // Add JWT Bearer authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = Microsoft.AspNetCore.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(context =>
        Microsoft.AspNetCore.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new Microsoft.AspNetCore.RateLimiting.FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();

// Middleware pipeline - Order matters!
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<IdempotencyMiddleware>();
app.UseMiddleware<Infrastructure.Middleware.PerEndpointRateLimitingMiddleware>();
app.UseCors("AllowedOrigins");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Prometheus metrics endpoint - must be after MapControllers
app.MapPrometheusScrapingEndpoint();

// Health check endpoints - Required for K8S
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new { status = "Alive" });
        await context.Response.WriteAsync(result);
    }
});

app.Run();

