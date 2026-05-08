using CoreApp.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreApp.Module;

public static class AppCoreModule
{
    public static IServiceCollection AddAppCoreModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddValidatorsFromAssemblyContaining<ParkingGateValidator>();
        services.AddFluentValidationAutoValidation();
        return services;
    }
}