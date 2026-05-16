using CoreApp.Module;
using CoreApp.Services;
using Infrastructure;
using Infrastructure.Security;
using WebApi.Exceptions;

namespace WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<JwtSettings>();
        builder.Services.AddAppCoreModule(builder.Configuration);
        builder.Services.AddParkingEfModule(builder.Configuration);
        builder.Services.AddJwt(new JwtSettings(builder.Configuration));
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
        builder.Services.AddProblemDetails();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            using var scope = app.Services.CreateScope();
            var seeders = scope.ServiceProvider
                .GetServices<IDataSeeder>()
                .OrderBy(s => s.Order);
            foreach (var seeder in seeders)
                await seeder.SeedAsync();
        }

        app.UseHttpsRedirection();
        app.UseExceptionHandler();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}