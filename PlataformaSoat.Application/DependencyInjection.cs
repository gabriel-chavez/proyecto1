using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using PlataformaSoat.Application.Modulos.Cobranza.Services;

namespace PlataformaSoat.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AutoMapper


        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Services
        services.AddScoped<CobranzaService>();

        return services;
    }
}
