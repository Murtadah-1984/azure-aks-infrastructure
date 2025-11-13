using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Features.OAuth2.GrantTypes;

/// <summary>
/// Factory for creating grant type handlers (Factory Pattern)
/// </summary>
public class GrantTypeHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GrantTypeHandlerFactory> _logger;

    public GrantTypeHandlerFactory(
        IServiceProvider serviceProvider,
        ILogger<GrantTypeHandlerFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IGrantTypeHandler GetHandler(string grantType)
    {
        return grantType.ToLowerInvariant() switch
        {
            "authorization_code" => _serviceProvider.GetRequiredService<AuthorizationCodeGrantHandler>(),
            "client_credentials" => _serviceProvider.GetRequiredService<ClientCredentialsGrantHandler>(),
            "refresh_token" => _serviceProvider.GetRequiredService<RefreshTokenGrantHandler>(),
            "device_code" => throw new NotImplementedException("Device code grant type not yet implemented"),
            _ => throw new ArgumentException($"Unsupported grant type: {grantType}", nameof(grantType))
        };
    }
}

