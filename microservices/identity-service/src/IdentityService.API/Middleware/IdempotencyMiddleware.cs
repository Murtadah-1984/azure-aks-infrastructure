using Microsoft.AspNetCore.Http;
using System.Text;

namespace IdentityService.API.Middleware;

/// <summary>
/// Middleware to extract idempotency key from request headers and add to HttpContext
/// </summary>
public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private const string IdempotencyKeyHeader = "Idempotency-Key";
    private const string IdempotencyKeyItem = "IdempotencyKey";

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract idempotency key from header
        if (context.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var idempotencyKey))
        {
            if (!string.IsNullOrEmpty(idempotencyKey))
            {
                context.Items[IdempotencyKeyItem] = idempotencyKey.ToString();
            }
        }

        await _next(context);
    }
}

