using FluentValidation;

namespace IdentityService.Application.Features.Clients.Commands.CreateClient;

public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client ID is required")
            .MaximumLength(100).WithMessage("Client ID cannot exceed 100 characters")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Client ID can only contain letters, numbers, hyphens, and underscores");

        RuleFor(x => x.ClientSecret)
            .NotEmpty().WithMessage("Client secret is required")
            .MinimumLength(32).WithMessage("Client secret must be at least 32 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Client name is required")
            .MaximumLength(200).WithMessage("Client name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.AccessTokenLifetime)
            .GreaterThan(0).WithMessage("Access token lifetime must be greater than 0")
            .LessThanOrEqualTo(86400).WithMessage("Access token lifetime cannot exceed 86400 seconds (24 hours)");

        RuleFor(x => x.RefreshTokenLifetime)
            .GreaterThan(0).WithMessage("Refresh token lifetime must be greater than 0")
            .LessThanOrEqualTo(365).WithMessage("Refresh token lifetime cannot exceed 365 days");

        RuleForEach(x => x.RedirectUris)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Invalid redirect URI format");

        RuleForEach(x => x.PostLogoutRedirectUris)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Invalid post-logout redirect URI format");
    }
}

