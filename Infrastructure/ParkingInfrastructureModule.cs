using CoreApp.Authorization;
using CoreApp.Enums;
using CoreApp.Repositories;
using CoreApp.Services;
using Infrastructure.EntityFramework.Context;
using Infrastructure.EntityFramework.Entities;
using Infrastructure.EntityFramework.Repositories;
using Infrastructure.EntityFramework.UnitOfWork;
using Infrastructure.Memory;
using Infrastructure.Security;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure;

public static class ParkingInfrastructureModule
{
    public static IServiceCollection AddParkingEfModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ParkingDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("ParkingDb")));

        services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<ParkingDbContext>()
            .AddDefaultTokenProviders();

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
        services.AddScoped<IParkingEmployeeService, ParkingEmployeeService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IDataSeeder, IdentityDbSeeder>();
        services.AddScoped<IDataSeeder, ParkingDataSeeder>();

        return services;
    }

    public static IServiceCollection AddJwt(
        this IServiceCollection services,
        JwtSettings jwtSettings)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = jwtSettings.GetSymmetricKey(),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AppPolicies.AdminOnly.Name(), policy =>
                policy.RequireRole(UserRole.Administrator.ToString()));

            options.AddPolicy(AppPolicies.ParkingEmployeeOnly.Name(), policy =>
                policy.RequireRole(
                    UserRole.ParkingEmployee.ToString(),
                    UserRole.Administrator.ToString()));

            options.AddPolicy(AppPolicies.ActiveUser.Name(), policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("status", SystemUserStatus.Active.ToString()));

            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser().Build();

            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser().Build();
        });

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