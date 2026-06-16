using BalochiAcademy.Application.Complaints;
using FluentValidation;

namespace BalochiAcademy.Application.Complaints.Validators;

public class CreateComplaintRequestValidator : AbstractValidator<CreateComplaintRequest>
{
    private static readonly string[] ValidCategories =
        ["general", "technical", "content", "payment", "account", "other"];

    public CreateComplaintRequestValidator()
    {
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MinimumLength(10).MaximumLength(2000);
        RuleFor(x => x.Category)
            .Must(c => ValidCategories.Contains(c))
            .WithMessage($"Category must be one of: {string.Join(", ", ValidCategories)}.");
    }
}

public class ReplyComplaintRequestValidator : AbstractValidator<ReplyComplaintRequest>
{
    private static readonly string[] ValidStatuses = ["open", "in-progress", "resolved", "closed"];

    public ReplyComplaintRequestValidator()
    {
        RuleFor(x => x.AdminReply).NotEmpty().MinimumLength(5).MaximumLength(2000);
        RuleFor(x => x.Status)
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.");
    }
}
