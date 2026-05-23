using CoreApp.Dto;
using CoreApp.Entities;
using CoreApp.Enums;
using CoreApp.Exceptions;
using CoreApp.Services;
using Infrastructure.Memory;

namespace UnitTest.Services;

public class ParkingEmployeeServiceTest
{
    private const string EmployeeJanId = "EMP-JAN";
    private const string EmployeeAnnaId = "EMP-ANNA";

    private readonly Guid _gateId = Guid.NewGuid();
    private readonly MemoryParkingUnitOfWork _unit;
    private readonly ParkingEmployeeService _service;
    private readonly ParkingTariff _activeTariff;

    public ParkingEmployeeServiceTest()
    {
        var vehicles = new MemoryVehicleRepository();
        var sessions = new MemoryParkingSessionRepository();
        var gates = new MemoryParkingGateRepository();
        var captures = new MemoryCameraCaptureRepository();
        var tariffs = new MemoryParkingTariffRepository();
        _unit = new MemoryParkingUnitOfWork(vehicles, sessions, gates, captures, tariffs);

        gates.AddAsync(new ParkingGate
        {
            Id = _gateId, Name = "Wjazd Testowy", Type = GateType.Entry,
            Location = "Test", IsOperational = true
        }).GetAwaiter().GetResult();

        var allTariffs = tariffs.FindAllAsync().GetAwaiter().GetResult().ToList();
        foreach (var t in allTariffs)
            tariffs.RemoveByIdAsync(t.Id).GetAwaiter().GetResult();

        _activeTariff = new ParkingTariff
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            FreeParkingDuration = TimeSpan.FromMinutes(15),
            HourlyRate = 5.00m,
            DailyMaxRate = 50.00m,
            IsActive = true
        };
        tariffs.AddAsync(_activeTariff).GetAwaiter().GetResult();

        _service = new ParkingEmployeeService(_unit);
    }

    [Fact]
    public async Task RegisterManualEntry_CreatesVehicleAndSession_WithCreatedBy()
    {
        var dto = new ManualEntryDto("KR12345", "Skoda", "Czarny", _gateId);

        var result = await _service.RegisterManualEntryAsync(dto, EmployeeJanId);

        Assert.Equal("KR12345", result.LicensePlate);
        Assert.True(result.IsActive);
        var sessions = await _unit.Sessions.FindAllActiveAsync();
        Assert.Single(sessions);
        Assert.Equal(EmployeeJanId, sessions.Single().CreatedById);
    }

    [Fact]
    public async Task RegisterManualEntry_ReusesExistingVehicle()
    {
        await _service.RegisterManualEntryAsync(new ManualEntryDto("WA11122", "VW", "Biały", _gateId), EmployeeJanId);
        await _service.RegisterManualEntryAsync(new ManualEntryDto("WA11122", "VW", "Biały", _gateId), EmployeeJanId);

        var vehicles = await _unit.Vehicles.FindAllAsync();
        Assert.Single(vehicles);
    }

    [Fact]
    public async Task RegisterManualEntry_GateNotFound_Throws()
    {
        var dto = new ManualEntryDto("KR99999", "BMW", "Czarny", Guid.NewGuid());

        await Assert.ThrowsAsync<GateNotFoundException>(
            () => _service.RegisterManualEntryAsync(dto, EmployeeJanId));
    }

    [Fact]
    public async Task RegisterManualExit_UnderFreeTime_ReturnsZeroFee()
    {
        var entry = await _service.RegisterManualEntryAsync(
            new ManualEntryDto("KR55555", "Audi", "Szary", _gateId), EmployeeJanId);
        var session = (await _unit.Sessions.FindByIdAsync(entry.SessionId))!;
        session.EntryTime = DateTime.UtcNow.AddMinutes(-5);

        var result = await _service.RegisterManualExitAsync(
            entry.SessionId, new ManualExitDto(_gateId), EmployeeJanId, false);

        Assert.False(result.IsActive);
        Assert.Equal(0m, result.ParkingFee);
    }

    [Fact]
    public async Task RegisterManualExit_OverFreeTime_ChargesHourlyRate()
    {
        var entry = await _service.RegisterManualEntryAsync(
            new ManualEntryDto("KR66666", "Fiat", "Czerwony", _gateId), EmployeeJanId);
        var session = (await _unit.Sessions.FindByIdAsync(entry.SessionId))!;
        session.EntryTime = DateTime.UtcNow.AddMinutes(-90);

        var result = await _service.RegisterManualExitAsync(
            entry.SessionId, new ManualExitDto(_gateId), EmployeeJanId, false);

        Assert.Equal(10m, result.ParkingFee);
    }

    [Fact]
    public async Task RegisterManualExit_VeryLong_CappedAtDailyMax()
    {
        var entry = await _service.RegisterManualEntryAsync(
            new ManualEntryDto("KR77777", "Toyota", "Niebieski", _gateId), EmployeeJanId);
        var session = (await _unit.Sessions.FindByIdAsync(entry.SessionId))!;
        session.EntryTime = DateTime.UtcNow.AddDays(-2);

        var result = await _service.RegisterManualExitAsync(
            entry.SessionId, new ManualExitDto(_gateId), EmployeeJanId, false);

        Assert.Equal(_activeTariff.DailyMaxRate, result.ParkingFee);
    }

    [Fact]
    public async Task RegisterManualExit_OtherEmployee_NotAdmin_ThrowsForbidden()
    {
        var entry = await _service.RegisterManualEntryAsync(
            new ManualEntryDto("KR88888", "Opel", "Biały", _gateId), EmployeeJanId);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _service.RegisterManualExitAsync(entry.SessionId,
                new ManualExitDto(_gateId), EmployeeAnnaId, isAdmin: false));
    }

    [Fact]
    public async Task RegisterManualExit_OtherEmployee_AsAdmin_Allowed()
    {
        var entry = await _service.RegisterManualEntryAsync(
            new ManualEntryDto("KR12121", "Renault", "Zielony", _gateId), EmployeeJanId);

        var result = await _service.RegisterManualExitAsync(entry.SessionId,
            new ManualExitDto(_gateId), EmployeeAnnaId, isAdmin: true);

        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task RegisterManualExit_AlreadyClosed_Throws()
    {
        var entry = await _service.RegisterManualEntryAsync(
            new ManualEntryDto("KR13131", "Ford", "Szary", _gateId), EmployeeJanId);
        await _service.RegisterManualExitAsync(entry.SessionId,
            new ManualExitDto(_gateId), EmployeeJanId, false);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RegisterManualExitAsync(entry.SessionId,
                new ManualExitDto(_gateId), EmployeeJanId, false));
    }

    [Fact]
    public async Task RegisterManualExit_SessionNotFound_Throws()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.RegisterManualExitAsync(Guid.NewGuid(),
                new ManualExitDto(_gateId), EmployeeJanId, false));
    }

    [Fact]
    public async Task CloseSessionFree_SetsZeroFee_AndClosesSession()
    {
        var entry = await _service.RegisterManualEntryAsync(
            new ManualEntryDto("KR14141", "BMW", "Czarny", _gateId), EmployeeJanId);
        var session = (await _unit.Sessions.FindByIdAsync(entry.SessionId))!;
        session.EntryTime = DateTime.UtcNow.AddHours(-5);

        var result = await _service.CloseSessionFreeAsync(
            entry.SessionId, "Reklamacja klienta", EmployeeJanId, false);

        Assert.False(result.IsActive);
        Assert.Equal(0m, result.ParkingFee);
        Assert.NotNull(result.ExitTime);
    }

    [Fact]
    public async Task CloseSessionFree_OtherEmployee_NotAdmin_ThrowsForbidden()
    {
        var entry = await _service.RegisterManualEntryAsync(
            new ManualEntryDto("KR15151", "Audi", "Czerwony", _gateId), EmployeeJanId);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _service.CloseSessionFreeAsync(entry.SessionId, "test",
                EmployeeAnnaId, isAdmin: false));
    }

    [Fact]
    public async Task GetActiveSessions_ReturnsOnlyActive()
    {
        var a = await _service.RegisterManualEntryAsync(
            new ManualEntryDto("KR16161", "VW", "Biały", _gateId), EmployeeJanId);
        await _service.RegisterManualEntryAsync(
            new ManualEntryDto("KR17171", "Skoda", "Czarny", _gateId), EmployeeJanId);
        await _service.CloseSessionFreeAsync(a.SessionId, "x", EmployeeJanId, false);

        var active = await _service.GetActiveSessionsAsync();

        Assert.Single(active);
        Assert.Equal("KR17171", active.Single().LicensePlate);
    }

    [Fact]
    public async Task SearchByLicensePlate_OnlyReturnsActive()
    {
        var a = await _service.RegisterManualEntryAsync(
            new ManualEntryDto("KR18181", "Toyota", "Biały", _gateId), EmployeeJanId);
        await _service.CloseSessionFreeAsync(a.SessionId, "x", EmployeeJanId, false);
        await _service.RegisterManualEntryAsync(
            new ManualEntryDto("KR18181", "Toyota", "Biały", _gateId), EmployeeJanId);

        var found = await _service.SearchByLicensePlateAsync("KR18181");

        Assert.Single(found);
    }
}
