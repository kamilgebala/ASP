using CoreApp.Repositories;
using CoreApp.Services;
using Infrastructure.Memory;
using Infrastructure.Services;

namespace WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // Repositories
        builder.Services.AddSingleton<IVehicleRepository, MemoryVehicleRepository>();
        builder.Services.AddSingleton<IParkingGateRepository, MemoryParkingGateRepository>();
        builder.Services.AddSingleton<IParkingSessionRepository, MemoryParkingSessionRepository>();

        // Unit of Work
        builder.Services.AddSingleton<IParkingUnitOfWork, MemoryParkingUnitOfWork>();

        // Services
        builder.Services.AddSingleton<IParkingGateService, MemoryParkingGateService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
