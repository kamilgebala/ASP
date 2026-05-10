using CoreApp.Module;
using CoreApp.Repositories;
using CoreApp.Services;
using Infrastructure.Memory;
using Infrastructure.Services;
using WebApi.Exceptions;

namespace WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAppCoreModule(builder.Configuration);
        builder.Services.AddControllers();
        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi();

        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddSingleton<IVehicleRepository, MemoryVehicleRepository>();
        builder.Services.AddSingleton<IParkingGateRepository, MemoryParkingGateRepository>();
        builder.Services.AddSingleton<IParkingSessionRepository, MemoryParkingSessionRepository>();
        builder.Services.AddSingleton<ICameraCaptureRepository, MemoryCameraCaptureRepository>();
        builder.Services.AddSingleton<IParkingTariffRepository, MemoryParkingTariffRepository>();
        builder.Services.AddSingleton<IParkingUnitOfWork, MemoryParkingUnitOfWork>();
        builder.Services.AddSingleton<IParkingGateService, MemoryParkingGateService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
            app.MapOpenApi();

        app.UseHttpsRedirection();
        app.UseExceptionHandler();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}