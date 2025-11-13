using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Common.Models;
using IdentityService.Infrastructure.Services.MfaProviders;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Features.Mfa.Commands.SendMfaCode;

public class SendMfaCodeCommandHandler : IRequestHandler<SendMfaCodeCommand, Result<SendMfaCodeResponse>>
{
    private readonly MfaProviderFactory _mfaProviderFactory;
    private readonly ILogger<SendMfaCodeCommandHandler> _logger;

    public SendMfaCodeCommandHandler(
        MfaProviderFactory mfaProviderFactory,
        ILogger<SendMfaCodeCommandHandler> logger)
    {
        _mfaProviderFactory = mfaProviderFactory;
        _logger = logger;
    }

    public async Task<Result<SendMfaCodeResponse>> Handle(SendMfaCodeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get appropriate MFA provider
            var provider = _mfaProviderFactory.GetProvider(request.ProviderType);

            // Generate code
            var code = await provider.GenerateCodeAsync(request.UserId, request.Identifier, cancellationToken);

            // Send code
            var sent = await provider.SendCodeAsync(request.UserId, request.Identifier, code, cancellationToken);

            if (!sent)
            {
                return Result<SendMfaCodeResponse>.Failure($"Failed to send {request.ProviderType} MFA code");
            }

            _logger.LogInformation("MFA code sent via {ProviderType} to {Identifier} for user {UserId}",
                request.ProviderType, request.Identifier, request.UserId);

            return Result<SendMfaCodeResponse>.Success(new SendMfaCodeResponse(
                Sent: true,
                Message: $"{request.ProviderType} MFA code sent successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid MFA provider type: {ProviderType}", request.ProviderType);
            return Result<SendMfaCodeResponse>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending MFA code");
            return Result<SendMfaCodeResponse>.Failure("An error occurred while sending the MFA code");
        }
    }
}

