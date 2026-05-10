using CoreApp.Dto;
using FluentValidation;

namespace CoreApp.Validators;

public class CameraCaptureValidator : AbstractValidator<CreateCameraCaptureDto>
{
    public CameraCaptureValidator()
    {
        RuleFor(x => x.LicensePlate)
            .NotEmpty().WithMessage("Numer rejestracyjny jest wymagany.")
            .MaximumLength(10).WithMessage("Numer rejestracyjny nie może przekraczać 10 znaków.");

        RuleFor(x => x.DetectedBrand)
            .NotEmpty().WithMessage("Marka pojazdu jest wymagana.")
            .MaximumLength(50).WithMessage("Marka nie może przekraczać 50 znaków.");

        RuleFor(x => x.DetectedColor)
            .NotEmpty().WithMessage("Kolor pojazdu jest wymagany.")
            .MaximumLength(30).WithMessage("Kolor nie może przekraczać 30 znaków.");
    }
}