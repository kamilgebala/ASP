using CoreApp.Dto;
using CoreApp.Entities;
using CoreApp.Enums;
using CoreApp.Exceptions;
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
        return entity is null ? null : (ParkingGateDto)entity;
    }

    public async Task<ParkingGateDto?> GetByNameAsync(string name)
    {
        var entity = await unit.Gates.FindByNameAsync(name);
        return entity is null ? null : (ParkingGateDto)entity;
    }

    public async Task<ParkingGateDto> CreateAsync(CreateGateDto dto)
    {
        var entity = dto.ToEntity();
        await unit.Gates.AddAsync(entity);
        await unit.SaveChangesAsync();
        return (ParkingGateDto)entity;
    }

    public async Task<ParkingGateDto> UpdateAsync(Guid id, UpdateGateDto dto)
    {
        var entity = await unit.Gates.FindByIdAsync(id)
                     ?? throw new GateNotFoundException(id);
        entity.Name = dto.Name;
        entity.Type = Enum.Parse<GateType>(dto.Type);
        await unit.Gates.UpdateAsync(entity);
        await unit.SaveChangesAsync();
        return (ParkingGateDto)entity;
    }

    public async Task<ParkingGateDto> ChangeOperationalStatusAsync(Guid id, bool isOperational)
    {
        var entity = await unit.Gates.FindByIdAsync(id)
                     ?? throw new GateNotFoundException(id);
        entity.IsOperational = isOperational;
        await unit.Gates.UpdateAsync(entity);
        await unit.SaveChangesAsync();
        return (ParkingGateDto)entity;
    }

    public async Task<CameraCapture> AddCaptureAsync(Guid gateId, CreateCameraCaptureDto dto)
    {
        var gate = await unit.Gates.FindByIdWithCapturesAsync(gateId)
                   ?? throw new GateNotFoundException(gateId);

        var capture = new CameraCapture
        {
            Id = Guid.NewGuid(),
            GateId = gateId,
            GateName = gate.Name,
            LicensePlate = dto.LicensePlate,
            DetectedBrand = dto.DetectedBrand,
            DetectedColor = dto.DetectedColor,
            Type = dto.Type,
            ImagePath = dto.ImagePath,
            CapturedAt = DateTime.UtcNow
        };

        gate.CameraCaptures.Add(capture);
        await unit.Captures.AddAsync(capture);
        await unit.SaveChangesAsync();
        return capture;
    }

    public async Task RemoveCaptureAsync(Guid gateId, Guid captureId)
    {
        var gate = await unit.Gates.FindByIdWithCapturesAsync(gateId)
                   ?? throw new GateNotFoundException(gateId);

        var capture = gate.CameraCaptures.FirstOrDefault(c => c.Id == captureId)
                      ?? throw new CaptureNotFoundException(captureId);

        gate.CameraCaptures.Remove(capture);
        await unit.Captures.RemoveByIdAsync(captureId);
        await unit.SaveChangesAsync();
    }

    public async Task<IEnumerable<CameraCaptureDto>> GetCapturesAsync(Guid gateId)
    {
        var gate = await unit.Gates.FindByIdWithCapturesAsync(gateId)
                   ?? throw new GateNotFoundException(gateId);

        return gate.CameraCaptures.Select(c => new CameraCaptureDto(
            c.Id, c.LicensePlate, c.DetectedBrand, c.DetectedColor, c.Type, c.ImagePath, c.CapturedAt));
    }
}