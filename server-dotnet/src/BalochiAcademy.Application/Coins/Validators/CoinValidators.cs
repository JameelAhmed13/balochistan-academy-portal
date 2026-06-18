using BalochiAcademy.Application.Coins;
using FluentValidation;

namespace BalochiAcademy.Application.Coins.Validators;

public class UpsertPayoutAccountRequestValidator : AbstractValidator<UpsertPayoutAccountRequest>
{
    private static readonly string[] ValidServices = ["JazzCash", "EasyPaisa", "BankTransfer"];

    public UpsertPayoutAccountRequestValidator()
    {
        RuleFor(x => x.AccountName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AccountNo).NotEmpty().MaximumLength(50)
            .Matches(@"^[\d\-]+$").WithMessage("Account number must contain only digits and hyphens.");
        RuleFor(x => x.Service)
            .NotEmpty()
            .Must(s => ValidServices.Contains(s))
            .WithMessage($"Service must be one of: {string.Join(", ", ValidServices)}.");
    }
}

public class CreateWithdrawalRequestValidator : AbstractValidator<CreateWithdrawalRequest>
{
    public CreateWithdrawalRequestValidator()
    {
        RuleFor(x => x.AmountCoins)
            .GreaterThanOrEqualTo(300).WithMessage("Minimum withdrawal is 300 coins.")
            .LessThanOrEqualTo(100_000).WithMessage("Maximum withdrawal is 100,000 coins per request.");
    }
}

public class ProcessWithdrawalRequestValidator : AbstractValidator<ProcessWithdrawalRequest>
{
    private static readonly string[] ValidStatuses = ["approved", "rejected", "paid"];

    public ProcessWithdrawalRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.");
        RuleFor(x => x.AdminNote).MaximumLength(500).When(x => x.AdminNote != null);
    }
}
