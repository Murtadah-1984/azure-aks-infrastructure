using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Features.WebAuthn.Queries.GetAuthenticationChallenge;

public class GetAuthenticationChallengeQueryHandler : IRequestHandler<GetAuthenticationChallengeQuery, Result<object>>
{
    private readonly IWebAuthnService _webAuthnService;
    private readonly ILogger<GetAuthenticationChallengeQueryHandler> _logger;

    public GetAuthenticationChallengeQueryHandler(
        IWebAuthnService webAuthnService,
        ILogger<GetAuthenticationChallengeQueryHandler> logger)
    {
        _webAuthnService = webAuthnService;
        _logger = logger;
    }

    public async Task<Result<object>> Handle(GetAuthenticationChallengeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var challenge = await _webAuthnService.CreateAuthenticationChallengeAsync(
                request.UserId,
                cancellationToken);

            return Result<object>.Success(challenge);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "No WebAuthn credentials found for user: {UserId}", request.UserId);
            return Result<object>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating WebAuthn authentication challenge");
            return Result<object>.Failure("An error occurred while creating the authentication challenge");
        }
    }
}

