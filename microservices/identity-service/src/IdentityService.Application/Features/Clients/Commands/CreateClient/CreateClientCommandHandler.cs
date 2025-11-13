using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Common.Models;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Features.Clients.Commands.CreateClient;

public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, Result<CreateClientResponse>>
{
    private readonly IClientRepository _clientRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateClientCommandHandler> _logger;

    public CreateClientCommandHandler(
        IClientRepository clientRepository,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUserService,
        ILogger<CreateClientCommandHandler> logger)
    {
        _clientRepository = clientRepository;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<CreateClientResponse>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if client ID already exists
            if (await _clientRepository.ExistsByClientIdAsync(request.ClientId, cancellationToken))
            {
                return Result<CreateClientResponse>.Failure("Client ID already exists");
            }

            // Hash client secret
            var clientSecretHash = _passwordHasher.HashPassword(request.ClientSecret);

            // Get current user ID for audit
            Guid? createdByUserId = null;
            if (!string.IsNullOrEmpty(_currentUserService.UserId))
            {
                createdByUserId = Guid.Parse(_currentUserService.UserId);
            }

            // Create client
            var client = Client.Create(
                request.ClientId,
                clientSecretHash,
                request.Name,
                request.Description,
                request.RequireConsent,
                request.RequirePkce,
                request.AccessTokenLifetime,
                request.RefreshTokenLifetime,
                createdByUserId);

            // Set grant types
            var grantTypes = request.AllowedGrantTypes ?? new List<string> { "authorization_code", "refresh_token" };
            client.SetAllowedGrantTypes(grantTypes);

            // Set scopes
            var scopes = request.AllowedScopes ?? new List<string> { "openid", "profile", "email" };
            client.SetAllowedScopes(scopes);

            // Set redirect URIs
            var redirectUris = request.RedirectUris ?? new List<string>();
            client.SetRedirectUris(redirectUris);

            // Set post-logout redirect URIs
            var postLogoutRedirectUris = request.PostLogoutRedirectUris ?? new List<string>();
            client.SetPostLogoutRedirectUris(postLogoutRedirectUris);

            await _clientRepository.AddAsync(client, cancellationToken);

            _logger.LogInformation("OAuth2 client created: {ClientId}", request.ClientId);

            return Result<CreateClientResponse>.Success(new CreateClientResponse(
                client.Id,
                client.ClientId,
                client.Name));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating OAuth2 client");
            return Result<CreateClientResponse>.Failure("An error occurred while creating the client");
        }
    }
}

