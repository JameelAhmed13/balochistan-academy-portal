using BalochiAcademy.Application.Questions;
using FluentValidation;

namespace BalochiAcademy.Application.Questions.Validators;

public class CreateQuestionRequestValidator : AbstractValidator<CreateQuestionRequest>
{
    private static readonly string[] ValidKinds        = ["objective", "subjective"];
    private static readonly string[] ValidDifficulties = ["easy", "medium", "hard"];

    public CreateQuestionRequestValidator()
    {
        RuleFor(x => x.Kind)
            .NotEmpty()
            .Must(k => ValidKinds.Contains(k, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Kind must be 'objective' or 'subjective'.");

        RuleFor(x => x.Stem).NotEmpty().MaximumLength(2000);

        RuleFor(x => x.GradeCode).MaximumLength(10).When(x => x.GradeCode != null);

        RuleFor(x => x.Difficulty)
            .Must(d => d == null || ValidDifficulties.Contains(d, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Difficulty must be Easy, Medium, or Hard.");

        RuleFor(x => x.Marks).InclusiveBetween(1, 100).When(x => x.Marks.HasValue);
        RuleFor(x => x.ModelAnswer).MaximumLength(4000).When(x => x.ModelAnswer != null);
        RuleFor(x => x.Feedback).MaximumLength(1000).When(x => x.Feedback != null);
        RuleFor(x => x.SloCode).MaximumLength(50).When(x => x.SloCode != null);

        // Objective questions must have options and a correct index
        When(x => string.Equals(x.Kind, "objective", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.OptionsJson)
                .NotEmpty().WithMessage("Objective questions require options JSON.");
            RuleFor(x => x.CorrectIndex)
                .NotNull().WithMessage("Objective questions require a correct index.")
                .GreaterThanOrEqualTo(0);
        });
    }
}

public class UpdateQuestionRequestValidator : AbstractValidator<UpdateQuestionRequest>
{
    private static readonly string[] ValidDifficulties = ["easy", "medium", "hard"];

    public UpdateQuestionRequestValidator()
    {
        RuleFor(x => x.Stem).MaximumLength(2000).When(x => x.Stem != null);
        RuleFor(x => x.ModelAnswer).MaximumLength(4000).When(x => x.ModelAnswer != null);
        RuleFor(x => x.Feedback).MaximumLength(1000).When(x => x.Feedback != null);
        RuleFor(x => x.Marks).InclusiveBetween(1, 100).When(x => x.Marks.HasValue);
        RuleFor(x => x.Difficulty)
            .Must(d => d == null || ValidDifficulties.Contains(d, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Difficulty must be Easy, Medium, or Hard.");
        RuleFor(x => x.CorrectIndex).GreaterThanOrEqualTo(0).When(x => x.CorrectIndex.HasValue);
    }
}
