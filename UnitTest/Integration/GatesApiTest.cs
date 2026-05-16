using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CoreApp.Dto;
using WebApi;

namespace UnitTest.Integration;

public class GatesApiTest(ParkingAppTestFactory<Program> app)
    : IClassFixture<ParkingAppTestFactory<Program>>
{
    private readonly HttpClient _client = app.CreateClient();

    [Fact]
    public async Task GetGates_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/gates");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetGates_WithAdminToken_Returns200WithJsonContentType()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = "admin@parking.pl", Password = "Admin@123!" });
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var response = await _client.GetAsync("/api/gates");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("application/json",
            response.Content.Headers.GetValues("Content-Type").First());

        _client.DefaultRequestHeaders.Authorization = null;
    }
}