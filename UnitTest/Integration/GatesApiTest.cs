using System.Net;
using System.Net.Http.Headers;
using CoreApp.Dto;
using CoreApp.Enums;
using WebApi;

namespace UnitTest.Integration;

public class GatesApiTest(ParkingAppTestFactory<Program> app)
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

    private Task LoginAsAdminAsync() => LoginAsync("admin@parking.pl", "Admin@123!");
    private Task LoginAsEmployeeJanAsync() => LoginAsync("jan.kowalski@parking.pl", "Employee@123!");
    private Task LoginAsEmployeeAnnaAsync() => LoginAsync("anna.nowak@parking.pl", "Employee@123!");

    [Fact]
    public async Task GetGates_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/gates");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetGates_WithAdminToken_Returns200WithJson()
    {
        await LoginAsAdminAsync();
        var response = await _client.GetAsync("/api/gates");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("application/json",
            response.Content.Headers.GetValues("Content-Type").First());
    }

    [Fact]
    public async Task GetGates_AsEmployee_Returns403()
    {
        await LoginAsEmployeeJanAsync();
        var response = await _client.GetAsync("/api/gates");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateGate_AsAdmin_Returns201()
    {
        await LoginAsAdminAsync();
        var dto = new CreateGateDto("Wjazd Testowy", "Entry", "Lokalizacja testowa");

        var response = await _client.PostAsJsonAsync("/api/gates", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ParkingGateDto>();
        Assert.NotNull(created);
        Assert.Equal("Wjazd Testowy", created.Name);
    }

    [Fact]
    public async Task CreateGate_AsEmployee_Returns403()
    {
        await LoginAsEmployeeJanAsync();
        var dto = new CreateGateDto("Wjazd X", "Entry", "Test");
        var response = await _client.PostAsJsonAsync("/api/gates", dto);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateGate_AsAdmin_Returns200()
    {
        await LoginAsAdminAsync();
        var dto = new UpdateGateDto("Wjazd Zmieniony", "Entry");

        var response = await _client.PutAsJsonAsync($"/api/gates/{EntryGateId}", dto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<ParkingGateDto>();
        Assert.NotNull(updated);
        Assert.Equal("Wjazd Zmieniony", updated.Name);
    }

    [Fact]
    public async Task ChangeStatus_AsAdmin_Returns200()
    {
        await LoginAsAdminAsync();
        var response = await _client.PatchAsync($"/api/gates/{EntryGateId}/status?isOperational=false",
            content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        await LoginAsAdminAsync();
        var response = await _client.GetAsync($"/api/gates/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddCapture_AsEmployee_Returns201()
    {
        await LoginAsEmployeeJanAsync();
        var dto = new CreateCameraCaptureDto("ZX12345", "Toyota", "Czarny", CaptureType.Entry);

        var response = await _client.PostAsJsonAsync($"/api/gates/{EntryGateId}/captures", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetCaptures_ReturnsList()
    {
        await LoginAsEmployeeJanAsync();
        await _client.PostAsJsonAsync($"/api/gates/{EntryGateId}/captures",
            new CreateCameraCaptureDto("ZX22222", "Audi", "Szary", CaptureType.Entry));

        var response = await _client.GetAsync($"/api/gates/{EntryGateId}/captures");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var captures = await response.Content.ReadFromJsonAsync<List<CameraCaptureDto>>();
        Assert.NotNull(captures);
        Assert.NotEmpty(captures);
    }

    [Fact]
    public async Task RemoveCapture_ByOtherEmployee_Returns403()
    {
        await LoginAsEmployeeJanAsync();
        var addResponse = await _client.PostAsJsonAsync($"/api/gates/{EntryGateId}/captures",
            new CreateCameraCaptureDto("ZX33333", "BMW", "Czerwony", CaptureType.Entry));
        var capture = await addResponse.Content.ReadFromJsonAsync<CameraCaptureDto>();

        await LoginAsEmployeeAnnaAsync();
        var deleteResponse = await _client.DeleteAsync(
            $"/api/gates/{EntryGateId}/captures/{capture!.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task RemoveCapture_ByAdmin_Returns204()
    {
        await LoginAsEmployeeJanAsync();
        var addResponse = await _client.PostAsJsonAsync($"/api/gates/{EntryGateId}/captures",
            new CreateCameraCaptureDto("ZX44444", "Fiat", "Niebieski", CaptureType.Entry));
        var capture = await addResponse.Content.ReadFromJsonAsync<CameraCaptureDto>();

        await LoginAsAdminAsync();
        var deleteResponse = await _client.DeleteAsync(
            $"/api/gates/{EntryGateId}/captures/{capture!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task RemoveCapture_ByAuthor_Returns204()
    {
        await LoginAsEmployeeJanAsync();
        var addResponse = await _client.PostAsJsonAsync($"/api/gates/{EntryGateId}/captures",
            new CreateCameraCaptureDto("ZX55555", "Opel", "Zielony", CaptureType.Entry));
        var capture = await addResponse.Content.ReadFromJsonAsync<CameraCaptureDto>();

        var deleteResponse = await _client.DeleteAsync(
            $"/api/gates/{EntryGateId}/captures/{capture!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
