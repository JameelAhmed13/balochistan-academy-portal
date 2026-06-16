using BalochiAcademy.Application.Notifications;
using FluentValidation;

namespace BalochiAcademy.Application.Notifications.Validators;

public class SendNotificationRequestValidator : AbstractValidator<SendNotificationRequest>
{
    private static readonly string[] ValidTypes = ["info", "success", "warning", "error", "announcement"];

    public SendNotificationRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Body).MaximumLength(1000).When(x => x.Body != null);
        RuleFor(x => x.Type)
            .Must(t => ValidTypes.Contains(t))
            .WithMessage($"Type must be one of: {string.Join(", ", ValidTypes)}.");
        RuleFor(x => x.Icon).MaximumLength(100).When(x => x.Icon != null);
        RuleFor(x => x.TargetRole).MaximumLength(50).When(x => x.TargetRole != null);
        RuleFor(x => x.TargetGrade).MaximumLength(10).When(x => x.TargetGrade != null);
        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiry date must be in the future.")
            .When(x => x.ExpiresAt.HasValue);
    }
}
