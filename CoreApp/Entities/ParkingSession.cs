namespace CoreApp.Entities;

public class ParkingSession : EntityBase
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    public string GateName { get; set; } = string.Empty;
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public decimal? ParkingFee { get; set; }
    public bool IsActive { get; set; }
}
