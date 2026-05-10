using CoreApp.Dto;
using CoreApp.Entities;
using CoreApp.Repositories;

namespace CoreApp.Services;

public interface IParkingGateService
{
    Task<PagedResult<ParkingGateDto>> GetAllAsync(int page, int pageSize);
    Task<ParkingGateDto?> GetByIdAsync(Guid id);
    Task<ParkingGateDto?> GetByNameAsync(string name);
    Task<ParkingGateDto> CreateAsync(CreateGateDto dto);
    Task<ParkingGateDto> UpdateAsync(Guid id, UpdateGateDto dto);
    Task<ParkingGateDto> ChangeOperationalStatusAsync(Guid id, bool isOperational);
    Task<CameraCapture> AddCaptureAsync(Guid gateId, CreateCameraCaptureDto dto);
    Task RemoveCaptureAsync(Guid gateId, Guid captureId);
    Task<IEnumerable<CameraCaptureDto>> GetCapturesAsync(Guid gateId);
}