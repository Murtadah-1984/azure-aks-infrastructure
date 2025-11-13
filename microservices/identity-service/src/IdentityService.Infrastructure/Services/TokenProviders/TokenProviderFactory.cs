using IdentityService.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdentityService.Infrastructure.Services.TokenProviders;

/// <summary>
/// Factory for creating token providers (Factory Pattern)
/// </summary>
public class TokenProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TokenProviderFactory> _logger;

    public TokenProviderFactory(
        IServiceProvider serviceProvider,
        ILogger<TokenProviderFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public ITokenProvider GetTokenProvider(string? providerType = null)
    {
        // Default to JWT if not specified
        providerType ??= "JWT";

        return providerType.ToUpperInvariant() switch
        {
            "JWT" => _serviceProvider.GetRequiredService<JwtTokenProvider>(),
            "REFERENCE" => _serviceProvider.GetRequiredService<ReferenceTokenProvider>(),
            _ => throw new ArgumentException($"Unknown token provider type: {providerType}", nameof(providerType))
        };
    }

    public ITokenProvider GetTokenProviderForClient(string? clientTokenType = null)
    {
        // Client can specify token type, default to JWT
        return GetTokenProvider(clientTokenType);
    }
}

