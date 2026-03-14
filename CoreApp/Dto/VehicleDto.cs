namespace CoreApp.Dto;

public record VehicleDto(
    Guid Id,
    string LicensePlate,
    string Brand,
    string Color
);
