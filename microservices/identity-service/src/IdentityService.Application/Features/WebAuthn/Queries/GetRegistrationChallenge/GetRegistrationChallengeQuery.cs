using IdentityService.Application.Common.Models;
using MediatR;

namespace IdentityService.Application.Features.WebAuthn.Queries.GetRegistrationChallenge;

public record GetRegistrationChallengeQuery(
    Guid UserId,
    string UserName,
    string UserDisplayName) : IRequest<Result<object>>;

