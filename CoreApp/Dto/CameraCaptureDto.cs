using CoreApp.Enums;

namespace CoreApp.Dto;

public record CreateCameraCaptureDto(
    string LicensePlate,
    string DetectedBrand,
    string DetectedColor,
    CaptureType Type,
    string ImagePath = ""
);

public record CameraCaptureDto(
    Guid Id,
    string LicensePlate,
    string DetectedBrand,
    string DetectedColor,
    CaptureType Type,
    string ImagePath,
    DateTime CapturedAt
);