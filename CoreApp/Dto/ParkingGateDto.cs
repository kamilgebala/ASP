using CoreApp.Entities;
using CoreApp.Enums;

namespace CoreApp.Dto;

public record ParkingGateDto(
    Guid Id,
    string Name,
    string Type,
    string Location,
    bool IsOperational
);

public record CreateGateDto(
    string Name,
    string Type,
    string Location
)
{
    public ParkingGate ToEntity()
    {
        return new ParkingGate
        {
            Id = Guid.NewGuid(),
            Name = Name,
            Type = Enum.Parse<GateType>(Type),
            Location = Location,
            IsOperational = false
        };
    }
};
