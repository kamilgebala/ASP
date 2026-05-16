using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CoreApp.Dto;
using WebApi;

namespace UnitTest.Integration;

public class EmployeeApiTest(ParkingAppTestFactory<Program> app)
    : IClassFixture<ParkingAppTestFactory<Program>>
{
    private readonly HttpClient _client = app.CreateClient();

    private static readonly Guid EntryGateId = Guid.Parse("10000000-0000-0000-0000-000000000001");

    private async Task SetEmployeeTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = "jan.kowalski@parking.pl", Password = "Employee@123!" });
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
    }

    private async Task SetDriverTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = "kierowca1@parking.pl", Password = "Driver@123!" });
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
    }

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
        await SetEmployeeTokenAsync();
        var response = await _client.GetAsync("/api/employee/sessions/active");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var sessions = await response.Content.ReadFromJsonAsync<List<ActiveSessionDto>>();
        Assert.NotNull(sessions);
    }

    [Fact]
    public async Task RegisterManualEntry_WithEmployeeToken_Returns201WithSession()
    {
        await SetEmployeeTokenAsync();
        var dto = new ManualEntryDto("KR12345", "Toyota", "Red", EntryGateId);
        var response = await _client.PostAsJsonAsync("/api/employee/sessions/entry", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var session = await response.Content.ReadFromJsonAsync<ParkingSessionDto>();
        Assert.NotNull(session);
        Assert.Equal("KR12345", session.LicensePlate);
        Assert.True(session.IsActive);
        Assert.Null(session.ExitTime);
    }

    [Fact]
    public async Task GetActiveSessions_AfterEntry_ContainsNewSession()
    {
        await SetEmployeeTokenAsync();

        await _client.PostAsJsonAsync("/api/employee/sessions/entry",
            new ManualEntryDto("KR99999", "BMW", "Blue", EntryGateId));

        var response = await _client.GetAsync("/api/employee/sessions/active");
        var sessions = await response.Content.ReadFromJsonAsync<List<ActiveSessionDto>>();
        Assert.NotNull(sessions);
        Assert.Contains(sessions, s => s.LicensePlate == "KR99999");
    }

    [Fact]
    public async Task SearchByLicensePlate_ExistingPlate_ReturnsSession()
    {
        await SetEmployeeTokenAsync();
        const string plate = "KR55555";
        await _client.PostAsJsonAsync("/api/employee/sessions/entry",
            new ManualEntryDto(plate, "Audi", "Black", EntryGateId));

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
        await SetEmployeeTokenAsync();
        var response = await _client.GetAsync("/api/employee/sessions/search?plate=NIEISTNIEJACA");
        var sessions = await response.Content.ReadFromJsonAsync<List<ActiveSessionDto>>();
        Assert.NotNull(sessions);
        Assert.Empty(sessions);
    }

    [Fact]
    public async Task RegisterManualExit_Returns200WithFee()
    {
        await SetEmployeeTokenAsync();
        var entryResponse = await _client.PostAsJsonAsync("/api/employee/sessions/entry",
            new ManualEntryDto("KR77777", "Ford", "White", EntryGateId));
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
        await SetEmployeeTokenAsync();
        var entryResponse = await _client.PostAsJsonAsync("/api/employee/sessions/entry",
            new ManualEntryDto("KR88888", "Skoda", "Green", EntryGateId));
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
    public async Task GetActiveSessions_AfterClosingAll_ReturnsEmptyList()
    {
        await SetEmployeeTokenAsync();
        const string plate = "KR11111";
        var entryResponse = await _client.PostAsJsonAsync("/api/employee/sessions/entry",
            new ManualEntryDto(plate, "VW", "Gray", EntryGateId));
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
        await SetDriverTokenAsync();
        var dto = new ManualEntryDto("KR00000", "Honda", "Yellow", EntryGateId);
        var response = await _client.PostAsJsonAsync("/api/employee/sessions/entry", dto);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}