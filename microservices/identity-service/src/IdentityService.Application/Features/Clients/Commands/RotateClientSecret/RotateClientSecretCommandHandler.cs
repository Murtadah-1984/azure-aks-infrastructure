using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Common.Models;
using IdentityService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Features.Clients.Commands.RotateClientSecret;

public class RotateClientSecretCommandHandler : IRequestHandler<RotateClientSecretCommand, Result<RotateClientSecretResponse>>
{
    private readonly IClientRepository _clientRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<RotateClientSecretCommandHandler> _logger;

    public RotateClientSecretCommandHandler(
        IClientRepository clientRepository,
        IPasswordHasher passwordHasher,
        ILogger<RotateClientSecretCommandHandler> logger)
    {
        _clientRepository = clientRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result<RotateClientSecretResponse>> Handle(RotateClientSecretCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var client = await _clientRepository.GetByIdAsync(request.ClientId, cancellationToken);
            if (client == null)
            {
                return Result<RotateClientSecretResponse>.Failure("Client not found");
            }

            // Hash new client secret
            var newSecretHash = _passwordHasher.HashPassword(request.NewClientSecret);

            // Rotate secret
            client.RotateSecret(newSecretHash);
            await _clientRepository.UpdateAsync(client, cancellationToken);

            _logger.LogInformation("Client secret rotated for client: {ClientId}", client.ClientId);

            return Result<RotateClientSecretResponse>.Success(new RotateClientSecretResponse(
                client.Id,
                client.ClientId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating client secret");
            return Result<RotateClientSecretResponse>.Failure("An error occurred while rotating the client secret");
        }
    }
}

