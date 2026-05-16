using CoreApp.Enums;
using CoreApp.Services;
using Infrastructure.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Security;

public class IdentityDbSeeder(
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager,
    ILogger<IdentityDbSeeder> logger) : IDataSeeder
{
    public int Order => 1;

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedUsersAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[]
        {
            new AppRole(UserRole.Administrator.ToString(), "Pełny dostęp do systemu."),
            new AppRole(UserRole.ParkingEmployee.ToString(), "Pracownik parkingu."),
            new AppRole(UserRole.Driver.ToString(), "Kierowca.")
        };

        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role.Name!)) continue;
            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
                logger.LogError("Błąd tworzenia roli {Role}: {Errors}", role.Name,
                    string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    private async Task SeedUsersAsync()
    {
        var users = new[]
        {
            new SeedUser("F5BADE14-6CC8-42A2-9A44-9842DA2D9280", "admin@parking.pl",
                "Adam", "Administrator", "IT", "Admin@123!", UserRole.Administrator),
            new SeedUser("93A7FFDD-057F-4021-9C68-FE06951FFA65", "jan.kowalski@parking.pl",
                "Jan", "Kowalski", "Parking", "Employee@123!", UserRole.ParkingEmployee),
            new SeedUser("3D4769E2-1C75-43E1-A5BB-1F71C68E9F57", "anna.nowak@parking.pl",
                "Anna", "Nowak", "Parking", "Employee@123!", UserRole.ParkingEmployee),
            new SeedUser("0E136AB2-1A6A-4A16-938D-84DFB0F64BBA", "kierowca1@parking.pl",
                "Piotr", "Wisniewski", "N/A", "Driver@123!", UserRole.Driver),
            new SeedUser("76B253D6-C16C-470A-943C-92F314A090F2", "kierowca2@parking.pl",
                "Maria", "Wojcik", "N/A", "Driver@123!", UserRole.Driver)
        };

        foreach (var seedUser in users)
            await CreateUserAsync(seedUser);
    }

    private async Task CreateUserAsync(SeedUser s)
    {
        if (await userManager.FindByEmailAsync(s.Email) is not null) return;

        var user = new AppUser
        {
            Id = s.Id,
            UserName = s.Email,
            Email = s.Email,
            NormalizedEmail = s.Email.ToUpper(),
            EmailConfirmed = true,
            FirstName = s.FirstName,
            LastName = s.LastName,
            FullName = $"{s.FirstName} {s.LastName}",
            Department = s.Department,
            Status = SystemUserStatus.Inactive,
            LockoutEnabled = true
        };
        user.Activate();

        var result = await userManager.CreateAsync(user, s.Password);
        if (!result.Succeeded)
        {
            logger.LogError("Błąd tworzenia {Email}: {Errors}", s.Email,
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(user, s.Role.ToString());
        logger.LogInformation("Utworzono {Email} z rolą {Role}.", s.Email, s.Role);
    }
}

internal record SeedUser(
    string Id, string Email,
    string FirstName, string LastName,
    string Department, string Password,
    UserRole Role);