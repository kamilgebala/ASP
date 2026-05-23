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

    public static class GateIds
    {
        public static readonly Guid EntryMain = Guid.Parse("10000000-0000-0000-0000-000000000001");
        public static readonly Guid ExitMain = Guid.Parse("10000000-0000-0000-0000-000000000002");
        public static readonly Guid EntrySide = Guid.Parse("10000000-0000-0000-0000-000000000003");
        public static readonly Guid ExitSide = Guid.Parse("10000000-0000-0000-0000-000000000004");
    }

    public static class TariffIds
    {
        public static readonly Guid Standard = Guid.Parse("20000000-0000-0000-0000-000000000001");
        public static readonly Guid Weekend = Guid.Parse("20000000-0000-0000-0000-000000000002");
        public static readonly Guid Premium = Guid.Parse("20000000-0000-0000-0000-000000000003");
    }

    public async Task SeedAsync()
    {
        await SeedGatesAsync();
        await SeedTariffsAsync();
        await SeedVehiclesAndSessionsAsync();
        await SeedCapturesAsync();
    }

    private async Task SeedGatesAsync()
    {
        if (await context.Gates.AnyAsync()) return;

        context.Gates.AddRange(
            new ParkingGate
            {
                Id = GateIds.EntryMain,
                Name = "Wjazd Główny",
                Type = GateType.Entry,
                Location = "Brama główna, ul. Parkingowa 1",
                IsOperational = true
            },
            new ParkingGate
            {
                Id = GateIds.ExitMain,
                Name = "Wyjazd Główny",
                Type = GateType.Exit,
                Location = "Brama główna, ul. Parkingowa 1",
                IsOperational = true
            },
            new ParkingGate
            {
                Id = GateIds.EntrySide,
                Name = "Wjazd Boczny",
                Type = GateType.Entry,
                Location = "Wjazd od ul. Krakowskiej",
                IsOperational = true
            },
            new ParkingGate
            {
                Id = GateIds.ExitSide,
                Name = "Wyjazd Boczny",
                Type = GateType.Exit,
                Location = "Wyjazd na ul. Krakowską",
                IsOperational = false
            });

        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} parking gates.", 4);
    }

    private async Task SeedTariffsAsync()
    {
        if (await context.Tariffs.AnyAsync()) return;

        context.Tariffs.AddRange(
            new ParkingTariff
            {
                Id = TariffIds.Standard,
                Name = "Standardowa",
                FreeParkingDuration = TimeSpan.FromMinutes(15),
                HourlyRate = 5.00m,
                DailyMaxRate = 50.00m,
                IsActive = true
            },
            new ParkingTariff
            {
                Id = TariffIds.Weekend,
                Name = "Weekendowa",
                FreeParkingDuration = TimeSpan.FromMinutes(30),
                HourlyRate = 4.00m,
                DailyMaxRate = 40.00m,
                IsActive = false
            },
            new ParkingTariff
            {
                Id = TariffIds.Premium,
                Name = "Premium",
                FreeParkingDuration = TimeSpan.FromHours(1),
                HourlyRate = 8.00m,
                DailyMaxRate = 80.00m,
                IsActive = false
            });

        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} parking tariffs.", 3);
    }

    private async Task SeedVehiclesAndSessionsAsync()
    {
        if (await context.Vehicles.AnyAsync()) return;

        var now = DateTime.UtcNow;
        var vehicles = new[]
        {
            new Vehicle { Id = VehicleIds.V01, LicensePlate = "KR12345", Brand = "Skoda",      Color = "Czarny"   },
            new Vehicle { Id = VehicleIds.V02, LicensePlate = "KR67890", Brand = "Toyota",     Color = "Czerwony" },
            new Vehicle { Id = VehicleIds.V03, LicensePlate = "WA11122", Brand = "Volkswagen", Color = "Biały"    },
            new Vehicle { Id = VehicleIds.V04, LicensePlate = "WA22233", Brand = "BMW",        Color = "Granatowy"},
            new Vehicle { Id = VehicleIds.V05, LicensePlate = "GD33344", Brand = "Audi",       Color = "Szary"    },
            new Vehicle { Id = VehicleIds.V06, LicensePlate = "GD44455", Brand = "Opel",       Color = "Srebrny"  },
            new Vehicle { Id = VehicleIds.V07, LicensePlate = "PO55566", Brand = "Fiat",       Color = "Czerwony" },
            new Vehicle { Id = VehicleIds.V08, LicensePlate = "PO66677", Brand = "Ford",       Color = "Niebieski"},
            new Vehicle { Id = VehicleIds.V09, LicensePlate = "WR77788", Brand = "Renault",    Color = "Biały"    },
            new Vehicle { Id = VehicleIds.V10, LicensePlate = "WR88899", Brand = "Peugeot",    Color = "Zielony"  },
            new Vehicle { Id = VehicleIds.V11, LicensePlate = "LU99900", Brand = "Mazda",      Color = "Czarny"   },
            new Vehicle { Id = VehicleIds.V12, LicensePlate = "LU10001", Brand = "Hyundai",    Color = "Szary"    }
        };
        context.Vehicles.AddRange(vehicles);

        var sessions = new[]
        {
            new ParkingSession
            {
                Id = SessionIds.Active1, VehicleId = VehicleIds.V01,
                GateName = "Wjazd Główny", EntryTime = now.AddMinutes(-12),
                IsActive = true, CreatedById = IdentityDbSeeder.UserIds.EmployeeJan
            },
            new ParkingSession
            {
                Id = SessionIds.Active2, VehicleId = VehicleIds.V02,
                GateName = "Wjazd Boczny", EntryTime = now.AddMinutes(-47),
                IsActive = true, CreatedById = IdentityDbSeeder.UserIds.EmployeeAnna
            },
            new ParkingSession
            {
                Id = SessionIds.Active3, VehicleId = VehicleIds.V03,
                GateName = "Wjazd Główny", EntryTime = now.AddHours(-3).AddMinutes(-15),
                IsActive = true, CreatedById = IdentityDbSeeder.UserIds.EmployeeMaria
            },
            new ParkingSession
            {
                Id = SessionIds.Closed1, VehicleId = VehicleIds.V04,
                GateName = "Wjazd Główny", EntryTime = now.AddHours(-26),
                ExitTime = now.AddHours(-24), ParkingFee = 10.00m, IsActive = false,
                CreatedById = IdentityDbSeeder.UserIds.EmployeeJan
            },
            new ParkingSession
            {
                Id = SessionIds.Closed2, VehicleId = VehicleIds.V05,
                GateName = "Wjazd Boczny", EntryTime = now.AddDays(-2).AddHours(-5),
                ExitTime = now.AddDays(-2), ParkingFee = 25.00m, IsActive = false,
                CreatedById = IdentityDbSeeder.UserIds.EmployeeAnna
            },
            new ParkingSession
            {
                Id = SessionIds.Closed3, VehicleId = VehicleIds.V06,
                GateName = "Wjazd Główny", EntryTime = now.AddDays(-3).AddHours(-12),
                ExitTime = now.AddDays(-3), ParkingFee = 50.00m, IsActive = false,
                CreatedById = IdentityDbSeeder.UserIds.EmployeeMaria
            }
        };
        context.Sessions.AddRange(sessions);

        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Vehicles} vehicles and {Sessions} sessions.",
            vehicles.Length, sessions.Length);
    }

    private async Task SeedCapturesAsync()
    {
        if (await context.Captures.AnyAsync()) return;

        var now = DateTime.UtcNow;
        var captures = new[]
        {
            MakeCapture(CaptureIds.C1, GateIds.EntryMain, "Wjazd Główny",
                "KR12345", "Skoda", "Czarny", CaptureType.Entry, now.AddMinutes(-12),
                IdentityDbSeeder.UserIds.EmployeeJan),
            MakeCapture(CaptureIds.C2, GateIds.EntrySide, "Wjazd Boczny",
                "KR67890", "Toyota", "Czerwony", CaptureType.Entry, now.AddMinutes(-47),
                IdentityDbSeeder.UserIds.EmployeeAnna),
            MakeCapture(CaptureIds.C3, GateIds.EntryMain, "Wjazd Główny",
                "WA11122", "Volkswagen", "Biały", CaptureType.Entry, now.AddHours(-3).AddMinutes(-15),
                IdentityDbSeeder.UserIds.EmployeeMaria),
            MakeCapture(CaptureIds.C4, GateIds.EntryMain, "Wjazd Główny",
                "WA22233", "BMW", "Granatowy", CaptureType.Entry, now.AddHours(-26),
                IdentityDbSeeder.UserIds.EmployeeJan),
            MakeCapture(CaptureIds.C5, GateIds.ExitMain, "Wyjazd Główny",
                "WA22233", "BMW", "Granatowy", CaptureType.Exit, now.AddHours(-24),
                IdentityDbSeeder.UserIds.EmployeeJan),
            MakeCapture(CaptureIds.C6, GateIds.EntrySide, "Wjazd Boczny",
                "GD33344", "Audi", "Szary", CaptureType.Entry, now.AddDays(-2).AddHours(-5),
                IdentityDbSeeder.UserIds.EmployeeAnna),
            MakeCapture(CaptureIds.C7, GateIds.ExitMain, "Wyjazd Główny",
                "GD33344", "Audi", "Szary", CaptureType.Exit, now.AddDays(-2),
                IdentityDbSeeder.UserIds.EmployeeAnna),
            MakeCapture(CaptureIds.C8, GateIds.EntryMain, "Wjazd Główny",
                "GD44455", "Opel", "Srebrny", CaptureType.Entry, now.AddDays(-3).AddHours(-12),
                IdentityDbSeeder.UserIds.EmployeeMaria)
        };

        context.Captures.AddRange(captures);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} camera captures.", captures.Length);
    }

    private static CameraCapture MakeCapture(Guid id, Guid gateId, string gateName,
        string plate, string brand, string color, CaptureType type, DateTime at, string createdBy)
        => new()
        {
            Id = id,
            GateId = gateId,
            GateName = gateName,
            LicensePlate = plate,
            DetectedBrand = brand,
            DetectedColor = color,
            Type = type,
            CapturedAt = at,
            ImagePath = $"/captures/{id}.jpg",
            CreatedById = createdBy
        };

    private static class VehicleIds
    {
        public static readonly Guid V01 = Guid.Parse("30000000-0000-0000-0000-000000000001");
        public static readonly Guid V02 = Guid.Parse("30000000-0000-0000-0000-000000000002");
        public static readonly Guid V03 = Guid.Parse("30000000-0000-0000-0000-000000000003");
        public static readonly Guid V04 = Guid.Parse("30000000-0000-0000-0000-000000000004");
        public static readonly Guid V05 = Guid.Parse("30000000-0000-0000-0000-000000000005");
        public static readonly Guid V06 = Guid.Parse("30000000-0000-0000-0000-000000000006");
        public static readonly Guid V07 = Guid.Parse("30000000-0000-0000-0000-000000000007");
        public static readonly Guid V08 = Guid.Parse("30000000-0000-0000-0000-000000000008");
        public static readonly Guid V09 = Guid.Parse("30000000-0000-0000-0000-000000000009");
        public static readonly Guid V10 = Guid.Parse("30000000-0000-0000-0000-000000000010");
        public static readonly Guid V11 = Guid.Parse("30000000-0000-0000-0000-000000000011");
        public static readonly Guid V12 = Guid.Parse("30000000-0000-0000-0000-000000000012");
    }

    private static class SessionIds
    {
        public static readonly Guid Active1 = Guid.Parse("40000000-0000-0000-0000-000000000001");
        public static readonly Guid Active2 = Guid.Parse("40000000-0000-0000-0000-000000000002");
        public static readonly Guid Active3 = Guid.Parse("40000000-0000-0000-0000-000000000003");
        public static readonly Guid Closed1 = Guid.Parse("40000000-0000-0000-0000-000000000011");
        public static readonly Guid Closed2 = Guid.Parse("40000000-0000-0000-0000-000000000012");
        public static readonly Guid Closed3 = Guid.Parse("40000000-0000-0000-0000-000000000013");
    }

    private static class CaptureIds
    {
        public static readonly Guid C1 = Guid.Parse("50000000-0000-0000-0000-000000000001");
        public static readonly Guid C2 = Guid.Parse("50000000-0000-0000-0000-000000000002");
        public static readonly Guid C3 = Guid.Parse("50000000-0000-0000-0000-000000000003");
        public static readonly Guid C4 = Guid.Parse("50000000-0000-0000-0000-000000000004");
        public static readonly Guid C5 = Guid.Parse("50000000-0000-0000-0000-000000000005");
        public static readonly Guid C6 = Guid.Parse("50000000-0000-0000-0000-000000000006");
        public static readonly Guid C7 = Guid.Parse("50000000-0000-0000-0000-000000000007");
        public static readonly Guid C8 = Guid.Parse("50000000-0000-0000-0000-000000000008");
    }
}
