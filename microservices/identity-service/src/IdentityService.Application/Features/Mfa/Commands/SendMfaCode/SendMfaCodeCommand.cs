using IdentityService.Application.Common.Models;
using MediatR;

namespace IdentityService.Application.Features.Mfa.Commands.SendMfaCode;

public record SendMfaCodeCommand(
    Guid UserId,
    string ProviderType, // "SMS" or "Email"
    string Identifier) // Phone number or email
    : IRequest<Result<SendMfaCodeResponse>>;

public record SendMfaCodeResponse(
    bool Sent,
    string? Message = null);

