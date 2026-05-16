namespace CoreApp.Dto;

public record ManualEntryDto(
    string LicensePlate,
    string Brand,
    string Color,
    Guid GateId,
    string? Note = null
);

public record ManualExitDto(
    Guid GateId,
    string? Note = null
);

public record ActiveSessionDto(
    Guid SessionId,
    string LicensePlate,
    string Brand,
    string Color,
    string GateName,
    DateTime EntryTime,
    int DurationMinutes
);

public record ParkingSessionDto(
    Guid SessionId,
    string LicensePlate,
    string Brand,
    string Color,
    string GateName,
    DateTime EntryTime,
    DateTime? ExitTime,
    decimal? ParkingFee,
    bool IsActive
);