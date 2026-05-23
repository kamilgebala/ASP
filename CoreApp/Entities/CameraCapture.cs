using CoreApp.Enums;

namespace CoreApp.Entities;

public class CameraCapture : EntityBase
{
    public Guid GateId { get; set; }
    public ParkingGate Gate { get; set; } = null!;
    public string GateName { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string DetectedBrand { get; set; } = string.Empty;
    public string DetectedColor { get; set; } = string.Empty;
    public DateTime CapturedAt { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public CaptureType Type { get; set; }
    public string? CreatedById { get; set; }
}