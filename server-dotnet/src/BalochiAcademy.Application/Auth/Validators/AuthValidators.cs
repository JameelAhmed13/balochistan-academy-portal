using BalochiAcademy.Application.Auth;
using FluentValidation;

namespace BalochiAcademy.Application.Auth.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Username)
            .NotEmpty().MinimumLength(3).MaximumLength(40)
            .Matches(@"^[a-zA-Z0-9_.\-]+$")
            .WithMessage("Username may only contain letters, numbers, underscores, dots, and hyphens.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(4).MaximumLength(100);
        RuleFor(x => x.GradeCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Phone).MaximumLength(20).When(x => x.Phone != null);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Institute).MaximumLength(200).When(x => x.Institute != null);
    }
}

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class UpdateGradeRequestValidator : AbstractValidator<UpdateGradeRequest>
{
    public UpdateGradeRequestValidator()
    {
        RuleFor(x => x.GradeCode).NotEmpty().MaximumLength(10);
    }
}
