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

    public static class UserIds
    {
        public const string Admin = "F5BADE14-6CC8-42A2-9A44-9842DA2D9280";
        public const string AdminAnna = "BA111111-0000-0000-0000-000000000001";
        public const string EmployeeJan = "93A7FFDD-057F-4021-9C68-FE06951FFA65";
        public const string EmployeeAnna = "3D4769E2-1C75-43E1-A5BB-1F71C68E9F57";
        public const string EmployeeMaria = "C0BB1234-AAAA-BBBB-CCCC-DDDDEEEEFFFF";
        public const string DriverPiotr = "0E136AB2-1A6A-4A16-938D-84DFB0F64BBA";
        public const string DriverMaria = "76B253D6-C16C-470A-943C-92F314A090F2";
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedUsersAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[]
        {
            new AppRole(UserRole.Administrator.ToString(), "Pełny dostęp do systemu parkingu."),
            new AppRole(UserRole.ParkingEmployee.ToString(), "Pracownik parkingu – obsługa wjazdów i wyjazdów."),
            new AppRole(UserRole.Driver.ToString(), "Kierowca korzystający z parkingu.")
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
            new SeedUser(UserIds.Admin, "admin@parking.pl",
                "Adam", "Administrator", "Administracja", "Admin@123!", UserRole.Administrator),
            new SeedUser(UserIds.AdminAnna, "anna.administrator@parking.pl",
                "Anna", "Lewandowska", "Administracja", "Admin@123!", UserRole.Administrator),
            new SeedUser(UserIds.EmployeeJan, "jan.kowalski@parking.pl",
                "Jan", "Kowalski", "Obsługa parkingu", "Employee@123!", UserRole.ParkingEmployee),
            new SeedUser(UserIds.EmployeeAnna, "anna.nowak@parking.pl",
                "Anna", "Nowak", "Obsługa parkingu", "Employee@123!", UserRole.ParkingEmployee),
            new SeedUser(UserIds.EmployeeMaria, "maria.wojcik@parking.pl",
                "Maria", "Wójcik", "Obsługa parkingu", "Employee@123!", UserRole.ParkingEmployee),
            new SeedUser(UserIds.DriverPiotr, "piotr.kierowca@parking.pl",
                "Piotr", "Wiśniewski", "Klient", "Driver@123!", UserRole.Driver),
            new SeedUser(UserIds.DriverMaria, "marta.kierowca@parking.pl",
                "Marta", "Kamińska", "Klient", "Driver@123!", UserRole.Driver)
        };

        foreach (var seedUser in users)
            await CreateUserAsync(seedUser);
    }

    private async Task CreateUserAsync(SeedUser s)
    {
        if (await userManager.FindByEmailAsync(s.Email) is not null) return;

        if (await userManager.FindByIdAsync(s.Id) is not null) return;

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
            Status = SystemUserStatus.Active,
            LockoutEnabled = true
        };

        var result = await userManager.CreateAsync(user, s.Password);
        if (!result.Succeeded)
        {
            logger.LogError("Błąd tworzenia użytkownika {Email}: {Errors}", s.Email,
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(user, s.Role.ToString());
        logger.LogInformation("Utworzono użytkownika {Email} z rolą {Role}.", s.Email, s.Role);
    }
}

internal record SeedUser(
    string Id, string Email,
    string FirstName, string LastName,
    string Department, string Password,
    UserRole Role);
