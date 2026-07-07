using BalochiAcademy.Application.Complaints;
using FluentValidation;

namespace BalochiAcademy.Application.Complaints.Validators;

public class CreateComplaintRequestValidator : AbstractValidator<CreateComplaintRequest>
{
    // Must match the "Type" options in the student-facing form (src/views/complaints/ComplaintsView.vue).
    private static readonly string[] ValidCategories =
        ["Complaint", "Suggestion", "Bug Report", "Feature Request", "General Feedback"];

    public CreateComplaintRequestValidator()
    {
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MinimumLength(10).MaximumLength(2000);
        RuleFor(x => x.Category)
            .Must(c => ValidCategories.Contains(c))
            .WithMessage($"Category must be one of: {string.Join(", ", ValidCategories)}.");
    }
}

public class SendComplaintMessageRequestValidator : AbstractValidator<SendComplaintMessageRequest>
{
    public SendComplaintMessageRequestValidator()
    {
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}

public class UpdateComplaintStatusRequestValidator : AbstractValidator<UpdateComplaintStatusRequest>
{
    private static readonly string[] ValidStatuses = ["open", "in-progress", "resolved", "closed"];

    public UpdateComplaintStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.");
    }
}
