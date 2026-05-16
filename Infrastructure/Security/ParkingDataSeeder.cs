using CoreApp.Entities;
using CoreApp.Enums;
using CoreApp.Services;
using Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Security;

public class ParkingDataSeeder(
    ParkingDbContext context,
    ILogger<ParkingDataSeeder> logger) : IDataSeeder
{
    public int Order => 2;

    public async Task SeedAsync()
    {
        await SeedGatesAsync();
        await SeedTariffsAsync();
    }

    private async Task SeedGatesAsync()
    {
        if (await context.Gates.AnyAsync()) return;

        context.Gates.AddRange(
            new ParkingGate
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Name = "Entry Gate",
                Type = GateType.Entry,
                Location = "Main Entrance",
                IsOperational = true
            },
            new ParkingGate
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Name = "Exit Gate",
                Type = GateType.Exit,
                Location = "Main Exit",
                IsOperational = true
            });

        await context.SaveChangesAsync();
        logger.LogInformation("Seeded parking gates.");
    }

    private async Task SeedTariffsAsync()
    {
        if (await context.Tariffs.AnyAsync()) return;

        context.Tariffs.AddRange(
            new ParkingTariff
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                Name = "Standard",
                FreeParkingDuration = TimeSpan.FromMinutes(15),
                HourlyRate = 5.00m,
                DailyMaxRate = 50.00m,
                IsActive = true
            },
            new ParkingTariff
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                Name = "Premium",
                FreeParkingDuration = TimeSpan.FromMinutes(30),
                HourlyRate = 8.00m,
                DailyMaxRate = 80.00m,
                IsActive = false
            });

        await context.SaveChangesAsync();
        logger.LogInformation("Seeded parking tariffs.");
    }
}