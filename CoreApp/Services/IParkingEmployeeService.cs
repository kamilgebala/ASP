using CoreApp.Dto;

namespace CoreApp.Services;

public interface IParkingEmployeeService
{
    Task<IEnumerable<ActiveSessionDto>> GetActiveSessionsAsync();
    Task<ParkingSessionDto> RegisterManualEntryAsync(ManualEntryDto dto, string userId);
    Task<ParkingSessionDto> RegisterManualExitAsync(Guid sessionId, ManualExitDto dto, string userId, bool isAdmin);
    Task<ParkingSessionDto> CloseSessionFreeAsync(Guid sessionId, string reason, string userId, bool isAdmin);
    Task<IEnumerable<ActiveSessionDto>> SearchByLicensePlateAsync(string plate);
}