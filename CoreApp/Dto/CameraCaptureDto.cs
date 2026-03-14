namespace CoreApp.Dto;

public record CameraCaptureDto(
    string LicensePlate,
    string Brand,
    string Color,
    string GateName,
    string? ImagePath = null
);
