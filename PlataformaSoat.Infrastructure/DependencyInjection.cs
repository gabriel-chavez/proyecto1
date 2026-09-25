using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlataformaSoat.Application.Modulos.Cobranza.Interfaces;
using PlataformaSoat.Application.Common.Interfaces;
using PlataformaSoat.Infrastructure.Configuration;
using PlataformaSoat.Infrastructure.Repositories;
using PlataformaSoat.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;
using Microsoft.EntityFrameworkCore;
using PlataformaSoat.Infrastructure.Persistence;

namespace PlataformaSoat.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuración
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetSection("PostgresSettings").Get<PostgresSettings>()?.ConnectionString ?? string.Empty));




        // Repositories
        services.AddScoped<ICobranzaRepository, CobranzaRepository>();

        // Cache (opcional)
        services.AddMemoryCache();
        services.AddScoped<ICacheService, CacheService>();

        return services;
    }
}
