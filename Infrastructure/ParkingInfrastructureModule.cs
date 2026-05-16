using CoreApp.Repositories;
using CoreApp.Services;
using Infrastructure.EntityFramework.Context;
using Infrastructure.EntityFramework.Repositories;
using Infrastructure.EntityFramework.UnitOfWork;
using Infrastructure.Memory;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class ParkingInfrastructureModule
{
    public static IServiceCollection AddParkingEfModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ParkingDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("ParkingDb")));

        services.AddScoped<EfParkingGateRepository>();
        services.AddScoped<EfParkingSessionRepository>();
        services.AddScoped<EfVehicleRepository>();
        services.AddScoped<EfCameraCaptureRepository>();
        services.AddScoped<EfParkingTariffRepository>();

        services.AddScoped<IParkingGateRepository>(sp => sp.GetRequiredService<EfParkingGateRepository>());
        services.AddScoped<IParkingSessionRepository>(sp => sp.GetRequiredService<EfParkingSessionRepository>());
        services.AddScoped<IVehicleRepository>(sp => sp.GetRequiredService<EfVehicleRepository>());
        services.AddScoped<ICameraCaptureRepository>(sp => sp.GetRequiredService<EfCameraCaptureRepository>());
        services.AddScoped<IParkingTariffRepository>(sp => sp.GetRequiredService<EfParkingTariffRepository>());

        services.AddScoped<IParkingUnitOfWork, EfParkingUnitOfWork>();
        services.AddScoped<IParkingGateService, ParkingGateService>();

        return services;
    }

    public static IServiceCollection AddParkingMemoryModule(this IServiceCollection services)
    {
        services.AddSingleton<IVehicleRepository, MemoryVehicleRepository>();
        services.AddSingleton<IParkingGateRepository, MemoryParkingGateRepository>();
        services.AddSingleton<IParkingSessionRepository, MemoryParkingSessionRepository>();
        services.AddSingleton<ICameraCaptureRepository, MemoryCameraCaptureRepository>();
        services.AddSingleton<IParkingTariffRepository, MemoryParkingTariffRepository>();
        services.AddSingleton<IParkingUnitOfWork, MemoryParkingUnitOfWork>();
        services.AddSingleton<IParkingGateService, MemoryParkingGateService>();
        return services;
    }
}