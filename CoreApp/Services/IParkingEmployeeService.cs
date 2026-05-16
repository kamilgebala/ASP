using CoreApp.Dto;

namespace CoreApp.Services;

public interface IParkingEmployeeService
{
    Task<IEnumerable<ActiveSessionDto>> GetActiveSessionsAsync();
    Task<ParkingSessionDto> RegisterManualEntryAsync(ManualEntryDto dto);
    Task<ParkingSessionDto> RegisterManualExitAsync(Guid sessionId, ManualExitDto dto);
    Task<ParkingSessionDto> CloseSessionFreeAsync(Guid sessionId, string reason);
    Task<IEnumerable<ActiveSessionDto>> SearchByLicensePlateAsync(string plate);
}