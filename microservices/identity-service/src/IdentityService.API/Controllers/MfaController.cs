using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Features.Mfa.Commands.EnableTotp;
using IdentityService.Application.Features.Mfa.Commands.EnrollTotp;
using IdentityService.Application.Features.Mfa.Commands.SendMfaCode;
using IdentityService.Application.Features.Mfa.Commands.VerifyMfaCode;
using IdentityService.Application.Features.Mfa.Commands.VerifyTotp;
using IdentityService.API.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

/// <summary>
/// Multi-factor authentication endpoints
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/mfa")]
[Produces("application/json")]
[Authorize]
public class MfaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MfaController> _logger;

    public MfaController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<MfaController> logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Enroll TOTP for MFA
    /// </summary>
    [HttpPost("totp/enroll")]
    [ProducesResponseType(typeof(ApiResponse<EnrollTotpResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnrollTotp(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            return Unauthorized();
        }

        var userId = Guid.Parse(_currentUserService.UserId);
        var command = new EnrollTotpCommand(userId);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<EnrollTotpResponse>.SuccessResponse(result.Value, "TOTP enrollment successful"));
    }

    /// <summary>
    /// Verify TOTP code
    /// </summary>
    [HttpPost("totp/verify")]
    [ProducesResponseType(typeof(ApiResponse<VerifyTotpResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyTotp(
        [FromBody] VerifyTotpRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            return Unauthorized();
        }

        var userId = Guid.Parse(_currentUserService.UserId);
        var command = new VerifyTotpCommand(userId, request.Code);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<VerifyTotpResponse>.SuccessResponse(result.Value));
    }

    /// <summary>
    /// Enable TOTP after verification
    /// </summary>
    [HttpPost("totp/enable")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnableTotp(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            return Unauthorized();
        }

        var userId = Guid.Parse(_currentUserService.UserId);
        var command = new EnableTotpCommand(userId);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, "TOTP enabled successfully"));
    }

    /// <summary>
    /// Send MFA code via SMS or Email
    /// </summary>
    [HttpPost("send-code")]
    [ProducesResponseType(typeof(ApiResponse<SendMfaCodeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendMfaCode(
        [FromBody] SendMfaCodeRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            return Unauthorized();
        }

        var userId = Guid.Parse(_currentUserService.UserId);
        var command = new SendMfaCodeCommand(userId, request.ProviderType, request.Identifier);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<SendMfaCodeResponse>.SuccessResponse(result.Value));
    }

    /// <summary>
    /// Verify MFA code (SMS or Email)
    /// </summary>
    [HttpPost("verify-code")]
    [ProducesResponseType(typeof(ApiResponse<VerifyMfaCodeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyMfaCode(
        [FromBody] VerifyMfaCodeRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            return Unauthorized();
        }

        var userId = Guid.Parse(_currentUserService.UserId);
        var command = new VerifyMfaCodeCommand(userId, request.ProviderType, request.Identifier ?? "", request.Code);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<VerifyMfaCodeResponse>.SuccessResponse(result.Value));
    }
}

public record SendMfaCodeRequest(string ProviderType, string Identifier);
public record VerifyMfaCodeRequest(string ProviderType, string? Identifier, string Code);

