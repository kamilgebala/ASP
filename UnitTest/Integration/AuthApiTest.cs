using System.Net;
using System.Net.Http.Json;
using CoreApp.Dto;
using WebApi;

namespace UnitTest.Integration;

public class AuthApiTest(ParkingAppTestFactory<Program> app)
    : IClassFixture<ParkingAppTestFactory<Program>>
{
    private readonly HttpClient _client = app.CreateClient();

    [Fact]
    public async Task Login_WithValidEmployeeCredentials_Returns200WithTokens()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = "jan.kowalski@parking.pl", Password = "Employee@123!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);
        Assert.NotEmpty(auth.AccessToken);
        Assert.NotEmpty(auth.RefreshToken);
        Assert.Equal("jan.kowalski@parking.pl", auth.User.Email);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = "jan.kowalski@parking.pl", Password = "WrongPassword!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithValidRefreshToken_Returns200WithNewToken()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = "jan.kowalski@parking.pl", Password = "Employee@123!" });
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenDto(auth!.AccessToken, auth.RefreshToken));

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var newAuth = await refreshResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotEmpty(newAuth!.AccessToken);
    }
}