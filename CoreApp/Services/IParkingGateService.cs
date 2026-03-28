using CoreApp.Dto;
using CoreApp.Repositories;

namespace CoreApp.Services;

public interface IParkingGateService
{
    Task<PagedResult<ParkingGateDto>> GetAllAsync(int page, int pageSize);
    Task<ParkingGateDto?> GetByIdAsync(Guid id);
    Task<ParkingGateDto?> GetByNameAsync(string name);
    Task<ParkingGateDto> CreateAsync(CreateGateDto dto);
    Task<ParkingGateDto> ChangeOperationalStatusAsync(Guid id, bool isOperational);
}