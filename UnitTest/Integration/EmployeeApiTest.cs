using System.Net;
using System.Net.Http.Headers;
using CoreApp.Dto;
using WebApi;

namespace UnitTest.Integration;

public class EmployeeApiTest(ParkingAppTestFactory<Program> app)
    : IClassFixture<ParkingAppTestFactory<Program>>
{
    private readonly HttpClient _client = app.CreateClient();

    private static readonly Guid EntryGateId = Guid.Parse("10000000-0000-0000-0000-000000000001");

    private async Task LoginAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = email, Password = password });
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
    }

    private Task LoginAsEmployeeJanAsync() =>
        LoginAsync("jan.kowalski@parking.pl", "Employee@123!");

    private Task LoginAsEmployeeAnnaAsync() =>
        LoginAsync("anna.nowak@parking.pl", "Employee@123!");

    private Task LoginAsAdminAsync() =>
        LoginAsync("admin@parking.pl", "Admin@123!");

    private Task LoginAsDriverAsync() =>
        LoginAsync("piotr.kierowca@parking.pl", "Driver@123!");

    [Fact]
    public async Task GetActiveSessions_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/employee/sessions/active");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetActiveSessions_WithEmployeeToken_Returns200WithList()
    {
        await LoginAsEmployeeJanAsync();
        var response = await _client.GetAsync("/api/employee/sessions/active");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var sessions = await response.Content.ReadFromJsonAsync<List<ActiveSessionDto>>();
        Assert.NotNull(sessions);
    }

    [Fact]
    public async Task RegisterManualEntry_WithEmployeeToken_Returns201WithSession()
    {
        await LoginAsEmployeeJanAsync();
        var dto = new ManualEntryDto("ZZ12345", "Toyota", "Czerwony", EntryGateId);
        var response = await _client.PostAsJsonAsync("/api/employee/sessions/entry", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var session = await response.Content.ReadFromJsonAsync<ParkingSessionDto>();
        Assert.NotNull(session);
        Assert.Equal("ZZ12345", session.LicensePlate);
        Assert.True(session.IsActive);
        Assert.Null(session.ExitTime);
    }

    [Fact]
    public async Task GetActiveSessions_AfterEntry_ContainsNewSession()
    {
        await LoginAsEmployeeJanAsync();

        await _client.PostAsJsonAsync("/api/employee/sessions/entry",
            new ManualEntryDto("ZZ99999", "BMW", "Niebieski", EntryGateId));

        var response = await _client.GetAsync("/api/employee/sessions/active");
        var sessions = await response.Content.ReadFromJsonAsync<List<ActiveSessionDto>>();
        Assert.NotNull(sessions);
        Assert.Contains(sessions, s => s.LicensePlate == "ZZ99999");
    }

    [Fact]
    public async Task SearchByLicensePlate_ExistingPlate_ReturnsSession()
    {
        await LoginAsEmployeeJanAsync();
        const string plate = "ZZ55555";
        await _client.PostAsJsonAsync("/api/employee/sessions/entry",
            new ManualEntryDto(plate, "Audi", "Czarny", EntryGateId));

        var response = await _client.GetAsync($"/api/employee/sessions/search?plate={plate}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var sessions = await response.Content.ReadFromJsonAsync<List<ActiveSessionDto>>();
        Assert.NotNull(sessions);
        Assert.Single(sessions);
        Assert.Equal(plate, sessions[0].LicensePlate);
    }

    [Fact]
    public async Task SearchByLicensePlate_NonExistingPlate_ReturnsEmptyList()
    {
        await LoginAsEmployeeJanAsync();
        var response = await _client.GetAsync("/api/employee/sessions/search?plate=NIEISTNIEJACA");
        var sessions = await response.Content.ReadFromJsonAsync<List<ActiveSessionDto>>();
        Assert.NotNull(sessions);
        Assert.Empty(sessions);
    }

    [Fact]
    public async Task RegisterManualExit_Returns200WithFee()
    {
        await LoginAsEmployeeJanAsync();
        var entryResponse = await _client.PostAsJsonAsync("/api/employee/sessions/entry",
            new ManualEntryDto("ZZ77777", "Ford", "Biały", EntryGateId));
        var session = await entryResponse.Content.ReadFromJsonAsync<ParkingSessionDto>();

        var exitResponse = await _client.PostAsJsonAsync(
            $"/api/employee/sessions/{session!.SessionId}/exit",
            new ManualExitDto(EntryGateId));

        Assert.Equal(HttpStatusCode.OK, exitResponse.StatusCode);
        var closed = await exitResponse.Content.ReadFromJsonAsync<ParkingSessionDto>();
        Assert.NotNull(closed);
        Assert.False(closed.IsActive);
        Assert.NotNull(closed.ExitTime);
        Assert.NotNull(closed.ParkingFee);
    }

    [Fact]
    public async Task CloseSessionFree_Returns200WithZeroFee()
    {
        await LoginAsEmployeeJanAsync();
        var entryResponse = await _client.PostAsJsonAsync("/api/employee/sessions/entry",
            new ManualEntryDto("ZZ88888", "Skoda", "Zielony", EntryGateId));
        var session = await entryResponse.Content.ReadFromJsonAsync<ParkingSessionDto>();

        var closeResponse = await _client.PostAsJsonAsync(
            $"/api/employee/sessions/{session!.SessionId}/close-free",
            "Reklamacja klienta");

        Assert.Equal(HttpStatusCode.OK, closeResponse.StatusCode);
        var closed = await closeResponse.Content.ReadFromJsonAsync<ParkingSessionDto>();
        Assert.NotNull(closed);
        Assert.False(closed.IsActive);
        Assert.Equal(0, closed.ParkingFee);
    }

    [Fact]
    public async Task GetActiveSessions_AfterClosingAll_ReturnsEmptyForThatPlate()
    {
        await LoginAsEmployeeJanAsync();
        const string plate = "ZZ11111";
        var entryResponse = await _client.PostAsJsonAsync("/api/employee/sessions/entry",
            new ManualEntryDto(plate, "VW", "Szary", EntryGateId));
        var session = await entryResponse.Content.ReadFromJsonAsync<ParkingSessionDto>();

        await _client.PostAsJsonAsync(
            $"/api/employee/sessions/{session!.SessionId}/close-free",
            "Test cleanup");

        var activeResponse = await _client.GetAsync("/api/employee/sessions/active");
        var active = await activeResponse.Content.ReadFromJsonAsync<List<ActiveSessionDto>>();
        Assert.NotNull(active);
        Assert.DoesNotContain(active, s => s.LicensePlate == plate);
    }

    [Fact]
    public async Task RegisterManualEntry_WithDriverToken_Returns403()
    {
        await LoginAsDriverAsync();
        var dto = new ManualEntryDto("ZZ00000", "Honda", "Żółty", EntryGateId);
        var response = await _client.PostAsJsonAsync("/api/employee/sessions/entry", dto);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CloseSessionFree_DifferentEmployeeThanAuthor_Returns403()
    {
        await LoginAsEmployeeJanAsync();
        var entry = await _client.PostAsJsonAsync("/api/employee/sessions/entry",
            new ManualEntryDto("ZZ22222", "Renault", "Biały", EntryGateId));
        var session = await entry.Content.ReadFromJsonAsync<ParkingSessionDto>();

        await LoginAsEmployeeAnnaAsync();
        var response = await _client.PostAsJsonAsync(
            $"/api/employee/sessions/{session!.SessionId}/close-free",
            "Test cudza sesja");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CloseSessionFree_AdminClosesAnyoneSession_Returns200()
    {
        await LoginAsEmployeeJanAsync();
        var entry = await _client.PostAsJsonAsync("/api/employee/sessions/entry",
            new ManualEntryDto("ZZ33333", "Peugeot", "Czarny", EntryGateId));
        var session = await entry.Content.ReadFromJsonAsync<ParkingSessionDto>();

        await LoginAsAdminAsync();
        var response = await _client.PostAsJsonAsync(
            $"/api/employee/sessions/{session!.SessionId}/close-free",
            "Admin override");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RegisterManualExit_SessionNotFound_Returns404()
    {
        await LoginAsEmployeeJanAsync();
        var response = await _client.PostAsJsonAsync(
            $"/api/employee/sessions/{Guid.NewGuid()}/exit",
            new ManualExitDto(EntryGateId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RegisterManualEntry_GateNotFound_Returns404()
    {
        await LoginAsEmployeeJanAsync();
        var response = await _client.PostAsJsonAsync("/api/employee/sessions/entry",
            new ManualEntryDto("ZZ44444", "Mazda", "Czerwony", Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
