using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Common.Models;
using IdentityService.Application.Features.OAuth2.GrantTypes;
using IdentityService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Features.OAuth2.Commands.Token;

public class TokenCommandHandler : IRequestHandler<TokenCommand, Result<TokenResponse>>
{
    private readonly IClientRepository _clientRepository;
    private readonly GrantTypeHandlerFactory _grantTypeHandlerFactory;
    private readonly ILogger<TokenCommandHandler> _logger;

    public TokenCommandHandler(
        IClientRepository clientRepository,
        GrantTypeHandlerFactory grantTypeHandlerFactory,
        ILogger<TokenCommandHandler> logger)
    {
        _clientRepository = clientRepository;
        _grantTypeHandlerFactory = grantTypeHandlerFactory;
        _logger = logger;
    }

    public async Task<Result<TokenResponse>> Handle(TokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate client
            if (string.IsNullOrEmpty(request.ClientId))
            {
                return Result<TokenResponse>.Failure("Client ID is required");
            }

            var client = await _clientRepository.GetByClientIdAsync(request.ClientId, cancellationToken);
            if (client == null || !client.IsActive)
            {
                return Result<TokenResponse>.Failure("Invalid or inactive client");
            }

            // Validate client secret (if provided)
            if (!string.IsNullOrEmpty(request.ClientSecret))
            {
                // In production, hash and compare client secret
                // For now, we'll skip this validation
            }

            // Check if grant type is allowed for this client
            if (!client.IsGrantTypeAllowed(request.GrantType))
            {
                return Result<TokenResponse>.Failure($"Grant type '{request.GrantType}' is not allowed for this client");
            }

            // Get appropriate grant type handler
            var handler = _grantTypeHandlerFactory.GetHandler(request.GrantType);

            // Create token request
            var tokenRequest = new TokenRequest
            {
                GrantType = request.GrantType,
                Code = request.Code,
                RedirectUri = request.RedirectUri,
                CodeVerifier = request.CodeVerifier,
                RefreshToken = request.RefreshToken,
                ClientId = request.ClientId,
                ClientSecret = request.ClientSecret,
                Scope = request.Scope
            };

            // Handle the grant type
            var result = await handler.HandleAsync(tokenRequest, client, cancellationToken);

            if (result.IsSuccess)
            {
                // Record client usage
                client.RecordUsage();
                await _clientRepository.UpdateAsync(client, cancellationToken);
            }

            return result;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid grant type: {GrantType}", request.GrantType);
            return Result<TokenResponse>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing token request");
            return Result<TokenResponse>.Failure("An error occurred while processing token request");
        }
    }
}

