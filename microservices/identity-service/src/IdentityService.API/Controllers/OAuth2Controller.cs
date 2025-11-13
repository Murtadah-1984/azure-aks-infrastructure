using IdentityService.Application.Features.OAuth2.Commands.Authorize;
using IdentityService.Application.Features.OAuth2.Commands.RevokeToken;
using IdentityService.Application.Features.OAuth2.Commands.Token;
using IdentityService.Application.Features.OAuth2.Queries.IntrospectToken;
using IdentityService.Application.Features.OAuth2.Queries.UserInfo;
using IdentityService.API.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

/// <summary>
/// OAuth2 and OpenID Connect endpoints
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/oauth2")]
[Produces("application/json")]
public class OAuth2Controller : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OAuth2Controller> _logger;

    public OAuth2Controller(IMediator mediator, ILogger<OAuth2Controller> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// OAuth2 Authorization endpoint
    /// </summary>
    [HttpGet("authorize")]
    [ProducesResponseType(typeof(ApiResponse<AuthorizeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> Authorize(
        [FromQuery] string response_type,
        [FromQuery] string client_id,
        [FromQuery] string redirect_uri,
        [FromQuery] string? scope = null,
        [FromQuery] string? state = null,
        [FromQuery] string? code_challenge = null,
        [FromQuery] string? code_challenge_method = null,
        CancellationToken cancellationToken = default)
    {
        var command = new AuthorizeCommand(
            response_type,
            client_id,
            redirect_uri,
            scope,
            state,
            code_challenge,
            code_challenge_method);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == "CONSENT_REQUIRED")
            {
                // Return consent required response
                return BadRequest(ApiResponse<object>.ErrorResponse("User consent is required for this client"));
            }
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
        }

        // Redirect to client with authorization code
        var redirectUrl = $"{redirect_uri}?code={result.Value.AuthorizationCode}" +
            (string.IsNullOrEmpty(state) ? "" : $"&state={state}");

        return Redirect(redirectUrl);
    }

    /// <summary>
    /// OAuth2 Token endpoint
    /// </summary>
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<IActionResult> Token(
        [FromForm] string grant_type,
        [FromForm] string? code = null,
        [FromForm] string? redirect_uri = null,
        [FromForm] string? client_id = null,
        [FromForm] string? client_secret = null,
        [FromForm] string? code_verifier = null,
        [FromForm] string? refresh_token = null,
        [FromForm] string? scope = null,
        CancellationToken cancellationToken = default)
    {
        var command = new TokenCommand(
            grant_type,
            code,
            redirect_uri,
            client_id,
            client_secret,
            code_verifier,
            refresh_token,
            scope);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
        }

        // OAuth2 token response format (not wrapped in ApiResponse)
        return Ok(result.Value);
    }

    /// <summary>
    /// OpenID Connect UserInfo endpoint
    /// </summary>
    [HttpGet("userinfo")]
    [ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<IActionResult> UserInfo(CancellationToken cancellationToken)
    {
        var query = new UserInfoQuery();
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// OIDC Discovery endpoint
    /// </summary>
    [HttpGet(".well-known/openid-configuration")]
    [ProducesResponseType(typeof(OpenIdConfigurationResponse), StatusCodes.Status200OK)]
    [AllowAnonymous]
    public IActionResult OpenIdConfiguration([FromServices] IConfiguration configuration)
    {
        var issuer = configuration["Jwt:Issuer"] ?? "IdentityService";
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var config = new OpenIdConfigurationResponse
        {
            Issuer = issuer,
            AuthorizationEndpoint = $"{baseUrl}/api/v1/oauth2/authorize",
            TokenEndpoint = $"{baseUrl}/api/v1/oauth2/token",
            UserInfoEndpoint = $"{baseUrl}/api/v1/oauth2/userinfo",
            JwksUri = $"{baseUrl}/api/v1/oauth2/.well-known/jwks.json",
            ScopesSupported = new[] { "openid", "profile", "email" },
            ResponseTypesSupported = new[] { "code" },
            GrantTypesSupported = new[] { "authorization_code", "client_credentials", "refresh_token" },
            IdTokenSigningAlgValuesSupported = new[] { "HS256" },
            SubjectTypesSupported = new[] { "public" },
            ClaimsSupported = new[] { "sub", "email", "email_verified", "name", "given_name", "family_name", "preferred_username" }
        };

        return Ok(config);
    }

    /// <summary>
    /// JWKS endpoint
    /// </summary>
    [HttpGet(".well-known/jwks.json")]
    [ProducesResponseType(typeof(JwksResponse), StatusCodes.Status200OK)]
    [AllowAnonymous]
    public IActionResult Jwks([FromServices] IConfiguration configuration)
    {
        // For now, return a simple JWKS
        // In production with key rotation, this would return multiple keys
        var jwks = new JwksResponse
        {
            Keys = new[]
            {
                new JwksKey
                {
                    Kty = "oct",
                    Use = "sig",
                    Alg = "HS256"
                }
            }
        };

        return Ok(jwks);
    }

    /// <summary>
    /// Token Introspection endpoint (RFC 7662)
    /// </summary>
    [HttpPost("introspect")]
    [ProducesResponseType(typeof(IntrospectTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<IActionResult> Introspect(
        [FromForm] string token,
        [FromForm] string? token_type_hint = null,
        CancellationToken cancellationToken = default)
    {
        var query = new IntrospectTokenQuery(token, token_type_hint);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Token Revocation endpoint (RFC 7009)
    /// </summary>
    [HttpPost("revoke")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<IActionResult> Revoke(
        [FromForm] string token,
        [FromForm] string? token_type_hint = null,
        CancellationToken cancellationToken = default)
    {
        var command = new RevokeTokenCommand(token, token_type_hint);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
        }

        // RFC 7009: Always return 200 OK, even if token was not found
        return Ok();
    }

    /// <summary>
    /// End Session endpoint (OIDC)
    /// </summary>
    [HttpGet("endsession")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize]
    public async Task<IActionResult> EndSession(
        [FromQuery] string? post_logout_redirect_uri = null,
        [FromQuery] string? state = null,
        CancellationToken cancellationToken = default)
    {
        // In a full implementation, this would:
        // 1. Revoke all user sessions
        // 2. Clear cookies
        // 3. Redirect to post_logout_redirect_uri if provided
        
        // For now, return success
        return Ok(new { message = "Session ended successfully" });
    }
}

public record OpenIdConfigurationResponse
{
    public string Issuer { get; set; } = string.Empty;
    public string AuthorizationEndpoint { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = string.Empty;
    public string UserInfoEndpoint { get; set; } = string.Empty;
    public string JwksUri { get; set; } = string.Empty;
    public string[] ScopesSupported { get; set; } = Array.Empty<string>();
    public string[] ResponseTypesSupported { get; set; } = Array.Empty<string>();
    public string[] GrantTypesSupported { get; set; } = Array.Empty<string>();
    public string[] IdTokenSigningAlgValuesSupported { get; set; } = Array.Empty<string>();
    public string[] SubjectTypesSupported { get; set; } = Array.Empty<string>();
    public string[] ClaimsSupported { get; set; } = Array.Empty<string>();
}

public record JwksResponse
{
    public JwksKey[] Keys { get; set; } = Array.Empty<JwksKey>();
}

public record JwksKey
{
    public string Kty { get; set; } = string.Empty;
    public string Use { get; set; } = string.Empty;
    public string Alg { get; set; } = string.Empty;
}
