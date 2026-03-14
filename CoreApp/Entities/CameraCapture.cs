using CoreApp.Enums;

namespace CoreApp.Entities;

public class CameraCapture : EntityBase
{
    public string GateName { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string DetectedBrand { get; set; } = string.Empty;
    public string DetectedColor { get; set; } = string.Empty;
    public DateTime CapturedAt { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public CaptureType Type { get; set; }
}
