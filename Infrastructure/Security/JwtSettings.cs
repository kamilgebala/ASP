using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Infrastructure.Security;

public class JwtSettings(IConfiguration configuration)
{
    private const string Section = "Jwt";
    public string Issuer => configuration[$"{Section}:Issuer"]
        ?? throw new InvalidOperationException("Jwt:Issuer is not set.");
    public string Audience => configuration[$"{Section}:Audience"]
        ?? throw new InvalidOperationException("Jwt:Audience is not set.");
    public string Secret => configuration[$"{Section}:SecretKey"]
        ?? throw new InvalidOperationException("Jwt:SecretKey is not set.");
    public int ExpirationInMinutes => configuration.GetValue<int>($"{Section}:ExpiryInMinutes");
    public int RefreshTokenDays => configuration.GetValue<int>($"{Section}:RefreshTokenDays");

    public SymmetricSecurityKey GetSymmetricKey() =>
        new(Encoding.UTF8.GetBytes(Secret));
}