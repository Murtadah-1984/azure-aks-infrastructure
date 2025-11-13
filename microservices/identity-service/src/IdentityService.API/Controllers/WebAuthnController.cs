using IdentityService.Application.Features.WebAuthn.Commands.AuthenticateWebAuthn;
using IdentityService.Application.Features.WebAuthn.Commands.RegisterWebAuthn;
using IdentityService.Application.Features.WebAuthn.Queries.GetAuthenticationChallenge;
using IdentityService.Application.Features.WebAuthn.Queries.GetRegistrationChallenge;
using IdentityService.API.Models;
using IdentityService.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

/// <summary>
/// WebAuthn/FIDO2 endpoints
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/webauthn")]
[Produces("application/json")]
public class WebAuthnController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<WebAuthnController> _logger;

    public WebAuthnController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<WebAuthnController> logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get registration challenge for WebAuthn credential registration
    /// </summary>
    [HttpPost("register/challenge")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> GetRegistrationChallenge(
        [FromBody] GetRegistrationChallengeRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            return Unauthorized();
        }

        var userId = Guid.Parse(_currentUserService.UserId);
        var query = new GetRegistrationChallengeQuery(userId, request.UserName, request.UserDisplayName);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<object>.SuccessResponse(result.Value));
    }

    /// <summary>
    /// Complete WebAuthn credential registration
    /// </summary>
    [HttpPost("register/complete")]
    [ProducesResponseType(typeof(ApiResponse<RegisterWebAuthnResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> Register(
        [FromBody] RegisterWebAuthnRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            return Unauthorized();
        }

        var userId = Guid.Parse(_currentUserService.UserId);
        var command = new RegisterWebAuthnCommand(
            userId,
            request.CredentialId,
            request.PublicKey,
            request.Counter,
            Convert.FromBase64String(request.AttestationObject),
            Convert.FromBase64String(request.ClientDataJson),
            request.Name);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<RegisterWebAuthnResponse>.SuccessResponse(result.Value, "WebAuthn credential registered successfully"));
    }

    /// <summary>
    /// Get authentication challenge for WebAuthn authentication
    /// </summary>
    [HttpPost("authenticate/challenge")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> GetAuthenticationChallenge(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            return Unauthorized();
        }

        var userId = Guid.Parse(_currentUserService.UserId);
        var query = new GetAuthenticationChallengeQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<object>.SuccessResponse(result.Value));
    }

    /// <summary>
    /// Complete WebAuthn authentication
    /// </summary>
    [HttpPost("authenticate/complete")]
    [ProducesResponseType(typeof(ApiResponse<AuthenticateWebAuthnResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> Authenticate(
        [FromBody] AuthenticateWebAuthnRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            return Unauthorized();
        }

        var userId = Guid.Parse(_currentUserService.UserId);
        var command = new AuthenticateWebAuthnCommand(
            userId,
            request.CredentialId,
            Convert.FromBase64String(request.AuthenticatorData),
            Convert.FromBase64String(request.ClientDataJson),
            Convert.FromBase64String(request.Signature),
            request.Counter);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<AuthenticateWebAuthnResponse>.SuccessResponse(result.Value));
    }
}

public record GetRegistrationChallengeRequest(string UserName, string UserDisplayName);
public record RegisterWebAuthnRequest(
    string CredentialId,
    string PublicKey,
    int Counter,
    string AttestationObject, // Base64 encoded
    string ClientDataJson, // Base64 encoded
    string? Name = null);
public record AuthenticateWebAuthnRequest(
    string CredentialId,
    string AuthenticatorData, // Base64 encoded
    string ClientDataJson, // Base64 encoded
    string Signature, // Base64 encoded
    int Counter);

