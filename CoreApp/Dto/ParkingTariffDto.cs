using CoreApp.Entities;

namespace CoreApp.Dto;

public record ParkingTariffDto(
    Guid Id,
    string Name,
    TimeSpan FreeParkingDuration,
    decimal HourlyRate,
    decimal DailyMaxRate,
    bool IsActive
);

public record CreateTariffDto(
    string Name,
    int FreeMinutes,
    decimal HourlyRate,
    decimal DailyMaxRate
)
{
    public ParkingTariff ToEntity() => new()
    {
        Name = Name,
        FreeParkingDuration = TimeSpan.FromMinutes(FreeMinutes),
        HourlyRate = HourlyRate,
        DailyMaxRate = DailyMaxRate,
        IsActive = false
    };
};
