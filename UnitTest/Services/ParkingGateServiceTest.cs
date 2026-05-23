using CoreApp.Dto;
using CoreApp.Enums;
using CoreApp.Exceptions;
using CoreApp.Services;
using Infrastructure.Memory;

namespace UnitTest.Services;

public class ParkingGateServiceTest
{
    private const string EmployeeJan = "EMP-JAN";
    private const string EmployeeAnna = "EMP-ANNA";

    private readonly MemoryParkingUnitOfWork _unit;
    private readonly ParkingGateService _service;

    public ParkingGateServiceTest()
    {
        var vehicles = new MemoryVehicleRepository();
        var sessions = new MemoryParkingSessionRepository();
        var gates = new MemoryParkingGateRepository();
        var captures = new MemoryCameraCaptureRepository();
        var tariffs = new MemoryParkingTariffRepository();
        _unit = new MemoryParkingUnitOfWork(vehicles, sessions, gates, captures, tariffs);
        _service = new ParkingGateService(_unit);

        var all = gates.FindAllAsync().GetAwaiter().GetResult().ToList();
        foreach (var g in all)
            gates.RemoveByIdAsync(g.Id).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task CreateAsync_AddsGate()
    {
        var dto = new CreateGateDto("Wjazd Główny", "Entry", "Brama A");

        var created = await _service.CreateAsync(dto);

        Assert.Equal("Wjazd Główny", created.Name);
        Assert.Equal("Entry", created.Type);
        Assert.False(created.IsOperational);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsGate_WhenExists()
    {
        var created = await _service.CreateAsync(new CreateGateDto("X", "Exit", "loc"));

        var found = await _service.GetByIdAsync(created.Id);

        Assert.NotNull(found);
        Assert.Equal(created.Id, found.Id);
    }

    [Fact]
    public async Task UpdateAsync_ChangesNameAndType()
    {
        var created = await _service.CreateAsync(new CreateGateDto("Old", "Entry", "loc"));

        var updated = await _service.UpdateAsync(created.Id, new UpdateGateDto("New", "Exit"));

        Assert.Equal("New", updated.Name);
        Assert.Equal("Exit", updated.Type);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_Throws()
    {
        await Assert.ThrowsAsync<GateNotFoundException>(() =>
            _service.UpdateAsync(Guid.NewGuid(), new UpdateGateDto("x", "Entry")));
    }

    [Fact]
    public async Task ChangeOperationalStatus_FlipsFlag()
    {
        var created = await _service.CreateAsync(new CreateGateDto("G", "Entry", "loc"));
        Assert.False(created.IsOperational);

        var updated = await _service.ChangeOperationalStatusAsync(created.Id, true);

        Assert.True(updated.IsOperational);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult()
    {
        await _service.CreateAsync(new CreateGateDto("A", "Entry", "x"));
        await _service.CreateAsync(new CreateGateDto("B", "Exit", "y"));
        await _service.CreateAsync(new CreateGateDto("C", "Entry", "z"));

        var page = await _service.GetAllAsync(page: 1, pageSize: 2);

        Assert.Equal(2, page.Items.Count);
        Assert.Equal(3, page.TotalCount);
    }

    [Fact]
    public async Task AddCaptureAsync_StoresWithCreatedBy()
    {
        var gate = await _service.CreateAsync(new CreateGateDto("G", "Entry", "x"));
        var dto = new CreateCameraCaptureDto("KR1", "Audi", "Czarny", CaptureType.Entry);

        var capture = await _service.AddCaptureAsync(gate.Id, dto, EmployeeJan);

        Assert.Equal("KR1", capture.LicensePlate);
        var stored = await _unit.Captures.FindByIdAsync(capture.Id);
        Assert.NotNull(stored);
        Assert.Equal(EmployeeJan, stored.CreatedById);
    }

    [Fact]
    public async Task GetCapturesAsync_ReturnsAllForGate()
    {
        var gate = await _service.CreateAsync(new CreateGateDto("G", "Entry", "x"));
        await _service.AddCaptureAsync(gate.Id, new CreateCameraCaptureDto("KR1", "A", "B", CaptureType.Entry), EmployeeJan);
        await _service.AddCaptureAsync(gate.Id, new CreateCameraCaptureDto("KR2", "A", "B", CaptureType.Entry), EmployeeJan);

        var list = (await _service.GetCapturesAsync(gate.Id)).ToList();

        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task RemoveCaptureAsync_Author_Succeeds()
    {
        var gate = await _service.CreateAsync(new CreateGateDto("G", "Entry", "x"));
        var capture = await _service.AddCaptureAsync(gate.Id,
            new CreateCameraCaptureDto("KR1", "A", "B", CaptureType.Entry), EmployeeJan);

        await _service.RemoveCaptureAsync(gate.Id, capture.Id, EmployeeJan, isAdmin: false);

        var list = await _service.GetCapturesAsync(gate.Id);
        Assert.Empty(list);
    }

    [Fact]
    public async Task RemoveCaptureAsync_OtherUser_NotAdmin_ThrowsForbidden()
    {
        var gate = await _service.CreateAsync(new CreateGateDto("G", "Entry", "x"));
        var capture = await _service.AddCaptureAsync(gate.Id,
            new CreateCameraCaptureDto("KR1", "A", "B", CaptureType.Entry), EmployeeJan);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _service.RemoveCaptureAsync(gate.Id, capture.Id, EmployeeAnna, isAdmin: false));
    }

    [Fact]
    public async Task RemoveCaptureAsync_OtherUser_AsAdmin_Succeeds()
    {
        var gate = await _service.CreateAsync(new CreateGateDto("G", "Entry", "x"));
        var capture = await _service.AddCaptureAsync(gate.Id,
            new CreateCameraCaptureDto("KR1", "A", "B", CaptureType.Entry), EmployeeJan);

        await _service.RemoveCaptureAsync(gate.Id, capture.Id, EmployeeAnna, isAdmin: true);

        Assert.Empty(await _service.GetCapturesAsync(gate.Id));
    }

    [Fact]
    public async Task RemoveCaptureAsync_GateNotFound_Throws()
    {
        await Assert.ThrowsAsync<GateNotFoundException>(() =>
            _service.RemoveCaptureAsync(Guid.NewGuid(), Guid.NewGuid(), EmployeeJan, false));
    }

    [Fact]
    public async Task RemoveCaptureAsync_CaptureNotFound_Throws()
    {
        var gate = await _service.CreateAsync(new CreateGateDto("G", "Entry", "x"));

        await Assert.ThrowsAsync<CaptureNotFoundException>(() =>
            _service.RemoveCaptureAsync(gate.Id, Guid.NewGuid(), EmployeeJan, false));
    }
}
