using IdentityService.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdentityService.Infrastructure.Services.MfaProviders;

/// <summary>
/// Factory for creating MFA providers (Factory Pattern)
/// </summary>
public class MfaProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MfaProviderFactory> _logger;

    public MfaProviderFactory(
        IServiceProvider serviceProvider,
        ILogger<MfaProviderFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IMfaProvider GetProvider(string providerType)
    {
        return providerType.ToUpperInvariant() switch
        {
            "TOTP" => _serviceProvider.GetRequiredService<TotpMfaProvider>(),
            "SMS" => _serviceProvider.GetRequiredService<SmsMfaProvider>(),
            "EMAIL" => _serviceProvider.GetRequiredService<EmailMfaProvider>(),
            _ => throw new ArgumentException($"Unknown MFA provider type: {providerType}", nameof(providerType))
        };
    }
}

