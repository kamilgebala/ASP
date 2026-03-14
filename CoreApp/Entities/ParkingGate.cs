using CoreApp.Enums;

namespace CoreApp.Entities;

public class ParkingGate : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public GateType Type { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool IsOperational { get; set; }
}
