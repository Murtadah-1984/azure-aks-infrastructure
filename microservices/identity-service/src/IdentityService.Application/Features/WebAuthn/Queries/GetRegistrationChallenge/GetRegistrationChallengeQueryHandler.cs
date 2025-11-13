using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Features.WebAuthn.Queries.GetRegistrationChallenge;

public class GetRegistrationChallengeQueryHandler : IRequestHandler<GetRegistrationChallengeQuery, Result<object>>
{
    private readonly IWebAuthnService _webAuthnService;
    private readonly ILogger<GetRegistrationChallengeQueryHandler> _logger;

    public GetRegistrationChallengeQueryHandler(
        IWebAuthnService webAuthnService,
        ILogger<GetRegistrationChallengeQueryHandler> logger)
    {
        _webAuthnService = webAuthnService;
        _logger = logger;
    }

    public async Task<Result<object>> Handle(GetRegistrationChallengeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var challenge = await _webAuthnService.CreateRegistrationChallengeAsync(
                request.UserId,
                request.UserName,
                request.UserDisplayName,
                cancellationToken);

            return Result<object>.Success(challenge);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating WebAuthn registration challenge");
            return Result<object>.Failure("An error occurred while creating the registration challenge");
        }
    }
}

