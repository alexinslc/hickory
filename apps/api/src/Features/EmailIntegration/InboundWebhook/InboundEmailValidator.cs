using FluentValidation;

namespace Hickory.Api.Features.EmailIntegration.InboundWebhook;

public class InboundEmailValidator : AbstractValidator<InboundEmailRequest>
{
    public InboundEmailValidator()
    {
        RuleFor(x => x.From)
            .NotEmpty().WithMessage("Sender email address is required")
            .EmailAddress().WithMessage("Sender must be a valid email address")
            .MaximumLength(320).WithMessage("Sender email cannot exceed 320 characters");

        RuleFor(x => x.To)
            .NotEmpty().WithMessage("Recipient email address is required")
            .MaximumLength(320).WithMessage("Recipient email cannot exceed 320 characters");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required")
            .MaximumLength(500).WithMessage("Subject cannot exceed 500 characters");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.TextBody) || !string.IsNullOrWhiteSpace(x.HtmlBody))
            .WithMessage("Email must have either a text body or HTML body");
    }
}
