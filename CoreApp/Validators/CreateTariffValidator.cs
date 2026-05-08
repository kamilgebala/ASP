using CoreApp.Dto;
using FluentValidation;

namespace CoreApp.Validators;

public class CreateTariffValidator : AbstractValidator<CreateTariffDto>
{
    public CreateTariffValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nazwa taryfy jest wymagana.")
            .MaximumLength(50).WithMessage("Nazwa nie może przekraczać 50 znaków.");

        RuleFor(x => x.FreeMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("Liczba darmowych minut nie może być ujemna.");

        RuleFor(x => x.HourlyRate)
            .GreaterThan(0).WithMessage("Stawka godzinowa musi być większa od zera.");

        RuleFor(x => x.DailyMaxRate)
            .GreaterThan(0).WithMessage("Maksymalna stawka dzienna musi być większa od zera.");
    }
}