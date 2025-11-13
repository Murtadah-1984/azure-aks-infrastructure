using IdentityService.Application.Common.Models;
using MediatR;

namespace IdentityService.Application.Features.WebAuthn.Queries.GetAuthenticationChallenge;

public record GetAuthenticationChallengeQuery(Guid UserId) : IRequest<Result<object>>;

