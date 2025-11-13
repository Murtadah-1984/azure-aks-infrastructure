using IdentityService.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;

namespace IdentityService.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior for idempotency support
/// Stores processed commands in Redis and returns cached results for duplicate requests
/// </summary>
public class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<IdempotencyBehavior<TRequest, TResponse>> _logger;

    public IdempotencyBehavior(
        IDistributedCache cache,
        ILogger<IdempotencyBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Check if request has idempotency key
        var idempotencyKey = GetIdempotencyKey(request);
        if (string.IsNullOrEmpty(idempotencyKey))
        {
            // No idempotency key, proceed normally
            return await next();
        }

        // Check cache for existing result
        var cacheKey = $"idempotency:{typeof(TRequest).Name}:{idempotencyKey}";
        var cachedResult = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedResult))
        {
            _logger.LogInformation("Idempotent request detected for key: {IdempotencyKey}", idempotencyKey);
            try
            {
                return JsonSerializer.Deserialize<TResponse>(cachedResult) 
                    ?? throw new InvalidOperationException("Failed to deserialize cached response");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize cached response, proceeding with new request");
                // If deserialization fails, proceed with new request
            }
        }

        // Execute request
        var response = await next();

        // Cache the result for 24 hours
        if (response != null)
        {
            try
            {
                var serializedResponse = JsonSerializer.Serialize(response);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                };
                await _cache.SetStringAsync(cacheKey, serializedResponse, options, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache idempotent response");
                // Continue even if caching fails
            }
        }

        return response;
    }

    private string? GetIdempotencyKey(TRequest request)
    {
        // Try to get idempotency key from request properties
        var requestType = typeof(TRequest);
        
        // Check for IdempotencyKey property
        var idempotencyKeyProperty = requestType.GetProperty("IdempotencyKey") 
            ?? requestType.GetProperty("IdempotencyKey", System.Reflection.BindingFlags.IgnoreCase);
        
        if (idempotencyKeyProperty != null)
        {
            var value = idempotencyKeyProperty.GetValue(request);
            return value?.ToString();
        }

        // Check for Idempotency-Key in HttpContext headers (for API requests)
        // This would require IHttpContextAccessor, but we'll handle it in the API layer
        return null;
    }
}

