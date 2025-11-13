using IdentityService.Application.Common.Models;
using MediatR;

namespace IdentityService.Application.Features.Mfa.Commands.VerifyMfaCode;

public record VerifyMfaCodeCommand(
    Guid UserId,
    string ProviderType, // "TOTP", "SMS", or "Email"
    string Identifier, // Phone number, email, or empty for TOTP
    string Code) : IRequest<Result<VerifyMfaCodeResponse>>;

public record VerifyMfaCodeResponse(
    bool IsValid,
    string? Message = null);

