using BalochiAcademy.Application.Tests;
using FluentValidation;

namespace BalochiAcademy.Application.Tests.Validators;

public class CreateTestRequestValidator : AbstractValidator<CreateTestRequest>
{
    private static readonly string[] ValidKinds = ["objective", "subjective", "mixed", "daily", "weekly", "entrance"];

    public CreateTestRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Kind)
            .NotEmpty()
            .Must(k => ValidKinds.Contains(k))
            .WithMessage($"Kind must be one of: {string.Join(", ", ValidKinds)}.");
        RuleFor(x => x.GradeCode).MaximumLength(10).When(x => x.GradeCode != null);
        RuleFor(x => x.DurationMin).InclusiveBetween(1, 300);
        RuleFor(x => x.TotalMarks).GreaterThan(0).When(x => x.TotalMarks.HasValue);
        RuleFor(x => x.QuestionIds)
            .Must(ids => ids == null || ids.Length > 0 || ids.Distinct().Count() == ids.Length)
            .WithMessage("Duplicate question IDs are not allowed.");
    }
}

public class UpdateTestRequestValidator : AbstractValidator<UpdateTestRequest>
{
    private static readonly string[] ValidKinds = ["objective", "subjective", "mixed", "daily", "weekly", "entrance"];

    public UpdateTestRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200).When(x => x.Title != null);
        RuleFor(x => x.Kind)
            .Must(k => k == null || ValidKinds.Contains(k))
            .WithMessage($"Kind must be one of: {string.Join(", ", ValidKinds)}.")
            .When(x => x.Kind != null);
        RuleFor(x => x.GradeCode).MaximumLength(10).When(x => x.GradeCode != null);
        RuleFor(x => x.DurationMin).InclusiveBetween(1, 300);
        RuleFor(x => x.TotalMarks).GreaterThan(0).When(x => x.TotalMarks.HasValue);
    }
}

public class SubmitAttemptRequestValidator : AbstractValidator<SubmitAttemptRequest>
{
    private static readonly string[] ValidAttemptTypes =
        ["self-test", "parent-test", "daily-test", "weekly-test", "entrance-test", "practice"];

    public SubmitAttemptRequestValidator()
    {
        RuleFor(x => x.Total).GreaterThan(0).WithMessage("Total questions must be greater than 0.");
        RuleFor(x => x.Score).GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(x => x.Total).WithMessage("Score cannot exceed total.");
        RuleFor(x => x.AttemptType)
            .NotEmpty()
            .Must(t => ValidAttemptTypes.Contains(t))
            .WithMessage($"AttemptType must be one of: {string.Join(", ", ValidAttemptTypes)}.");
        RuleFor(x => x.DurationSec).GreaterThanOrEqualTo(0);
    }
}
