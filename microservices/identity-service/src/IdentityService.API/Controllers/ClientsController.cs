using IdentityService.Application.Features.Clients.Commands.CreateClient;
using IdentityService.Application.Features.Clients.Commands.RotateClientSecret;
using IdentityService.Application.Features.Clients.Queries.GetClients;
using IdentityService.API.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

/// <summary>
/// OAuth2 client management endpoints
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/clients")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class ClientsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(IMediator mediator, ILogger<ClientsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get a paginated list of OAuth2 clients
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetClientsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetClients(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetClientsQuery(pageNumber, pageSize, searchTerm, isActive);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<GetClientsResponse>.SuccessResponse(result.Value));
    }

    /// <summary>
    /// Create a new OAuth2 client
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateClientResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateClient(
        [FromBody] CreateClientCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
        }

        return CreatedAtAction(
            nameof(GetClients),
            new { },
            ApiResponse<CreateClientResponse>.SuccessResponse(result.Value, "Client created successfully"));
    }

    /// <summary>
    /// Rotate client secret
    /// </summary>
    [HttpPost("{clientId:guid}/rotate-secret")]
    [ProducesResponseType(typeof(ApiResponse<RotateClientSecretResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RotateClientSecret(
        Guid clientId,
        [FromBody] RotateClientSecretRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RotateClientSecretCommand(clientId, request.NewClientSecret);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<RotateClientSecretResponse>.SuccessResponse(result.Value, "Client secret rotated successfully"));
    }
}

public record RotateClientSecretRequest(string NewClientSecret);

