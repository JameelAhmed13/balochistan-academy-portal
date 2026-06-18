using BalochiAcademy.Application.Grades;
using FluentValidation;

namespace BalochiAcademy.Application.Grades.Validators;

public class CreateGradeRequestValidator : AbstractValidator<CreateGradeRequest>
{
    public CreateGradeRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(10)
            .Matches(@"^[A-Za-z0-9\-]+$").WithMessage("Grade code must be alphanumeric.");
        RuleFor(x => x.Label).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Band).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateGradeRequestValidator : AbstractValidator<UpdateGradeRequest>
{
    public UpdateGradeRequestValidator()
    {
        RuleFor(x => x.Label).MaximumLength(50).When(x => x.Label != null);
        RuleFor(x => x.Band).MaximumLength(50).When(x => x.Band != null);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0).When(x => x.SortOrder.HasValue);
    }
}

public class SetGradeSubjectsRequestValidator : AbstractValidator<SetGradeSubjectsRequest>
{
    public SetGradeSubjectsRequestValidator()
    {
        RuleFor(x => x.SubjectIds).NotNull()
            .Must(ids => ids.Length > 0).WithMessage("At least one subject ID is required.")
            .Must(ids => ids.Distinct().Count() == ids.Length).WithMessage("Duplicate subject IDs are not allowed.");
    }
}
