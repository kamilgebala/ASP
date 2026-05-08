using CoreApp.Dto;
using FluentValidation;

namespace CoreApp.Validators;

public class CameraCaptureValidator : AbstractValidator<CameraCaptureDto>
{
    public CameraCaptureValidator()
    {
        RuleFor(x => x.LicensePlate)
            .NotEmpty().WithMessage("Numer rejestracyjny jest wymagany.")
            .MaximumLength(10).WithMessage("Numer rejestracyjny nie może przekraczać 10 znaków.")
            .Matches(@"^[A-Z0-9\s\-]+$").WithMessage("Numer rejestracyjny zawiera niedozwolone znaki.");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Marka pojazdu jest wymagana.")
            .MaximumLength(50).WithMessage("Marka nie może przekraczać 50 znaków.");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("Kolor pojazdu jest wymagany.")
            .MaximumLength(30).WithMessage("Kolor nie może przekraczać 30 znaków.");

        RuleFor(x => x.GateName)
            .NotEmpty().WithMessage("Nazwa bramki jest wymagana.")
            .MaximumLength(50).WithMessage("Nazwa bramki nie może przekraczać 50 znaków.");
    }
}