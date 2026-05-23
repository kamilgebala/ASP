using CoreApp.Dto;
using CoreApp.Entities;
using CoreApp.Exceptions;
using CoreApp.Repositories;

namespace CoreApp.Services;

public class ParkingEmployeeService(IParkingUnitOfWork unit) : IParkingEmployeeService
{
    public async Task<IEnumerable<ActiveSessionDto>> GetActiveSessionsAsync()
    {
        var sessions = await unit.Sessions.FindAllActiveAsync();
        return sessions.Select(ToActiveDto);
    }

    public async Task<ParkingSessionDto> RegisterManualEntryAsync(ManualEntryDto dto, string userId)
    {
        var gate = await unit.Gates.FindByIdAsync(dto.GateId)
                   ?? throw new GateNotFoundException(dto.GateId);

        var vehicle = await unit.Vehicles.FindByLicensePlateAsync(dto.LicensePlate);
        if (vehicle is null)
        {
            vehicle = new Vehicle
            {
                Id = Guid.NewGuid(),
                LicensePlate = dto.LicensePlate,
                Brand = dto.Brand,
                Color = dto.Color
            };
            await unit.Vehicles.AddAsync(vehicle);
        }

        var session = new ParkingSession
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            Vehicle = vehicle,
            GateName = gate.Name,
            EntryTime = DateTime.UtcNow,
            IsActive = true,
            CreatedById = userId
        };

        await unit.Sessions.AddAsync(session);
        await unit.SaveChangesAsync();
        return ToDto(session);
    }

    public async Task<ParkingSessionDto> RegisterManualExitAsync(
        Guid sessionId, ManualExitDto dto, string userId, bool isAdmin)
    {
        var session = await unit.Sessions.FindByIdAsync(sessionId)
                      ?? throw new KeyNotFoundException($"Sesja {sessionId} nie istnieje.");

        EnsureCanModify(session, userId, isAdmin);

        if (!session.IsActive)
            throw new InvalidOperationException("Sesja jest już zamknięta.");

        var tariff = await unit.Tariffs.FindActiveAsync();
        var exitTime = DateTime.UtcNow;
        var duration = exitTime - session.EntryTime;

        decimal fee = 0;
        if (tariff is not null && duration > tariff.FreeParkingDuration)
        {
            var chargeableHours = Math.Ceiling((duration - tariff.FreeParkingDuration).TotalHours);
            fee = Math.Min((decimal)chargeableHours * tariff.HourlyRate, tariff.DailyMaxRate);
        }

        session.ExitTime = exitTime;
        session.ParkingFee = fee;
        session.IsActive = false;
        await unit.Sessions.UpdateAsync(session);
        await unit.SaveChangesAsync();
        return ToDto(session);
    }

    public async Task<ParkingSessionDto> CloseSessionFreeAsync(
        Guid sessionId, string reason, string userId, bool isAdmin)
    {
        var session = await unit.Sessions.FindByIdAsync(sessionId)
                      ?? throw new KeyNotFoundException($"Sesja {sessionId} nie istnieje.");

        EnsureCanModify(session, userId, isAdmin);

        if (!session.IsActive)
            throw new InvalidOperationException("Sesja jest już zamknięta.");

        session.ExitTime = DateTime.UtcNow;
        session.ParkingFee = 0;
        session.IsActive = false;
        await unit.Sessions.UpdateAsync(session);
        await unit.SaveChangesAsync();
        return ToDto(session);
    }

    public async Task<IEnumerable<ActiveSessionDto>> SearchByLicensePlateAsync(string plate)
    {
        var sessions = await unit.Sessions.FindHistoryByLicensePlateAsync(plate);
        return sessions.Where(s => s.IsActive).Select(ToActiveDto);
    }

    private static void EnsureCanModify(ParkingSession session, string userId, bool isAdmin)
    {
        if (isAdmin) return;
        if (session.CreatedById == userId) return;
        throw new ForbiddenAccessException(
            "Sesję może modyfikować tylko pracownik, który ją utworzył, lub administrator.");
    }

    private static ActiveSessionDto ToActiveDto(ParkingSession s) => new(
        s.Id, s.Vehicle.LicensePlate, s.Vehicle.Brand, s.Vehicle.Color,
        s.GateName, s.EntryTime,
        (int)(DateTime.UtcNow - s.EntryTime).TotalMinutes);

    private static ParkingSessionDto ToDto(ParkingSession s) => new(
        s.Id, s.Vehicle.LicensePlate, s.Vehicle.Brand, s.Vehicle.Color,
        s.GateName, s.EntryTime, s.ExitTime, s.ParkingFee, s.IsActive);
}
