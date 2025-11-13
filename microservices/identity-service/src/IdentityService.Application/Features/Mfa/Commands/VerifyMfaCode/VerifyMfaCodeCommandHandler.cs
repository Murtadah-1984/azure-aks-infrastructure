using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Common.Models;
using IdentityService.Infrastructure.Services.MfaProviders;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Features.Mfa.Commands.VerifyMfaCode;

public class VerifyMfaCodeCommandHandler : IRequestHandler<VerifyMfaCodeCommand, Result<VerifyMfaCodeResponse>>
{
    private readonly MfaProviderFactory _mfaProviderFactory;
    private readonly ILogger<VerifyMfaCodeCommandHandler> _logger;

    public VerifyMfaCodeCommandHandler(
        MfaProviderFactory mfaProviderFactory,
        ILogger<VerifyMfaCodeCommandHandler> logger)
    {
        _mfaProviderFactory = mfaProviderFactory;
        _logger = logger;
    }

    public async Task<Result<VerifyMfaCodeResponse>> Handle(VerifyMfaCodeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get appropriate MFA provider
            var provider = _mfaProviderFactory.GetProvider(request.ProviderType);

            // Verify code
            var isValid = await provider.VerifyCodeAsync(
                request.UserId,
                request.Identifier,
                request.Code,
                cancellationToken);

            if (!isValid)
            {
                _logger.LogWarning("Invalid MFA code for user {UserId}, provider {ProviderType}",
                    request.UserId, request.ProviderType);
                return Result<VerifyMfaCodeResponse>.Success(new VerifyMfaCodeResponse(
                    IsValid: false,
                    Message: "Invalid MFA code"));
            }

            _logger.LogInformation("MFA code verified successfully for user {UserId}, provider {ProviderType}",
                request.UserId, request.ProviderType);

            return Result<VerifyMfaCodeResponse>.Success(new VerifyMfaCodeResponse(
                IsValid: true,
                Message: "MFA code verified successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid MFA provider type: {ProviderType}", request.ProviderType);
            return Result<VerifyMfaCodeResponse>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying MFA code");
            return Result<VerifyMfaCodeResponse>.Failure("An error occurred while verifying the MFA code");
        }
    }
}

