namespace CoreApp.Dto;

public record ParkingStatsDto(
    int ActiveVehicles,
    decimal TodayRevenue,
    int TodayEntries,
    int TodayExits
);
