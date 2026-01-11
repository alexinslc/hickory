using FluentValidation;
using Hickory.Api.Features.Auth.Models;

namespace Hickory.Api.Features.Auth.RefreshToken;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenRequest>
{
    private const int MinimumTokenLength = 32;
    
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required")
            .MinimumLength(MinimumTokenLength).WithMessage("Invalid refresh token format");
    }
}
