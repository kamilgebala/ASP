using System.Net;
using System.Net.Http.Headers;
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
        Assert.Contains("ParkingEmployee", auth.User.Roles);
    }

    [Fact]
    public async Task Login_WithValidAdminCredentials_HasAdminRole()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = "admin@parking.pl", Password = "Admin@123!" });

        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);
        Assert.Contains("Administrator", auth.User.Roles);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = "jan.kowalski@parking.pl", Password = "ZleHaslo!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = "nieistnieje@parking.pl", Password = "DowolneHaslo@123!" });

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
        Assert.NotEqual(auth.RefreshToken, newAuth.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithInvalidRefreshToken_Returns401()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = "jan.kowalski@parking.pl", Password = "Employee@123!" });
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenDto(auth!.AccessToken, "niepoprawny-token"));

        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsUserProfile()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = "anna.nowak@parking.pl", Password = "Employee@123!" });
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var meResponse = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        var me = await meResponse.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(me);
        Assert.Equal("anna.nowak@parking.pl", me.Email);
        Assert.Equal("Anna", me.FirstName);
        Assert.Contains("ParkingEmployee", me.Roles);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
