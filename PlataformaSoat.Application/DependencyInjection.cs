using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PlataformaSoat.Application.Modulos.Auth.Services;
using PlataformaSoat.Application.Modulos.Sales.Services;
using PlataformaSoat.Application.Modulos.SalesParams.Services;
using PlataformaSoat.Application.Modulos.SalesRecon.Services;

namespace PlataformaSoat.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Services
        services.AddScoped<AuthService>();
        services.AddScoped<SalesParamsService>();
        services.AddScoped<SalesService>();
        services.AddScoped<SalesReconService>();

        return services;
    }
}
