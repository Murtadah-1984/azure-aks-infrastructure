using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text;

namespace IdentityService.Infrastructure.Middleware;

/// <summary>
/// Per-endpoint rate limiting middleware using Redis
/// </summary>
public class PerEndpointRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;
    private readonly ILogger<PerEndpointRateLimitingMiddleware> _logger;

    // Rate limit configuration per endpoint
    private static readonly Dictionary<string, RateLimitConfig> RateLimits = new()
    {
        { "/api/v1/oauth2/token", new RateLimitConfig { Limit = 20, WindowMinutes = 1, KeyPrefix = "rate_limit:token" } },
        { "/api/v1/auth/register", new RateLimitConfig { Limit = 5, WindowMinutes = 1, KeyPrefix = "rate_limit:register" } },
        { "/api/v1/auth/login", new RateLimitConfig { Limit = 10, WindowMinutes = 1, KeyPrefix = "rate_limit:login" } },
        { "/api/v1/mfa/totp/verify", new RateLimitConfig { Limit = 3, WindowMinutes = 5, KeyPrefix = "rate_limit:mfa_verify" } }
    };

    public PerEndpointRateLimitingMiddleware(
        RequestDelegate next,
        IDistributedCache cache,
        ILogger<PerEndpointRateLimitingMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        
        // Check if this endpoint has rate limiting configured
        var rateLimitConfig = RateLimits.FirstOrDefault(kvp => path.Contains(kvp.Key.ToLowerInvariant()));
        
        if (rateLimitConfig.Value != null)
        {
            // Get rate limit key (IP address or user ID)
            var rateLimitKey = GetRateLimitKey(context, rateLimitConfig.Value.KeyPrefix);
            var cacheKey = $"{rateLimitKey}:{path}";

            // Get current count
            var currentCountStr = await _cache.GetStringAsync(cacheKey);
            var currentCount = string.IsNullOrEmpty(currentCountStr) ? 0 : int.Parse(currentCountStr);

            if (currentCount >= rateLimitConfig.Value.Limit)
            {
                _logger.LogWarning("Rate limit exceeded for endpoint: {Path}, key: {Key}", path, rateLimitKey);
                context.Response.StatusCode = 429; // Too Many Requests
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    System.Text.Json.JsonSerializer.Serialize(new
                    {
                        error = "rate_limit_exceeded",
                        message = $"Rate limit exceeded. Maximum {rateLimitConfig.Value.Limit} requests per {rateLimitConfig.Value.WindowMinutes} minute(s).",
                        retryAfter = rateLimitConfig.Value.WindowMinutes * 60
                    }),
                    Encoding.UTF8);
                return;
            }

            // Increment count
            currentCount++;
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(rateLimitConfig.Value.WindowMinutes)
            };
            await _cache.SetStringAsync(cacheKey, currentCount.ToString(), options);
        }

        await _next(context);
    }

    private string GetRateLimitKey(HttpContext context, string prefix)
    {
        // Try to get user ID first (for authenticated requests)
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"{prefix}:user:{userId}";
        }

        // Fall back to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"{prefix}:ip:{ipAddress}";
    }

    private class RateLimitConfig
    {
        public int Limit { get; set; }
        public int WindowMinutes { get; set; }
        public string KeyPrefix { get; set; } = string.Empty;
    }
}

