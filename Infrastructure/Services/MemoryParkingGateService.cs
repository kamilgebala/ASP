using CoreApp.Dto;
using CoreApp.Repositories;
using CoreApp.Services;

namespace Infrastructure.Services;

public class MemoryParkingGateService(IParkingUnitOfWork unit) : IParkingGateService
{
    public async Task<PagedResult<ParkingGateDto>> GetAllAsync(int page, int pageSize)
    {
        var pagedResult = await unit.Gates.FindPagedAsync(page, pageSize);
        var dtos = pagedResult.Items.Select(g => (ParkingGateDto)g).ToList();
        return new PagedResult<ParkingGateDto>(dtos, pagedResult.TotalCount, pagedResult.Page, pagedResult.PageSize);
    }

    public async Task<ParkingGateDto?> GetByIdAsync(Guid id)
    {
        var entity = await unit.Gates.FindByIdAsync(id);
        if (entity is null)
            return null;
        return (ParkingGateDto)entity;
    }

    public async Task<ParkingGateDto?> GetByNameAsync(string name)
    {
        var entity = await unit.Gates.FindByNameAsync(name);
        if (entity is null)
            return null;
        return (ParkingGateDto)entity;
    }

    public async Task<ParkingGateDto> CreateAsync(CreateGateDto dto)
    {
        var entity = dto.ToEntity();
        await unit.Gates.AddAsync(entity);
        await unit.SaveChangesAsync();
        return (ParkingGateDto)entity;
    }

    public async Task<ParkingGateDto> ChangeOperationalStatusAsync(Guid id, bool isOperational)
    {
        var entity = await unit.Gates.FindByIdAsync(id)
                     ?? throw new KeyNotFoundException($"ParkingGate with id '{id}' not found.");
        entity.IsOperational = isOperational;
        await unit.Gates.UpdateAsync(entity);
        await unit.SaveChangesAsync();
        return (ParkingGateDto)entity;
    }
}