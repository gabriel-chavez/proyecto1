using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlataformaSoat.Application.Common.Interfaces;
using PlataformaSoat.Application.Modulos.Auth.Interfaces;
using PlataformaSoat.Application.Modulos.Sales.Interfaces;
using PlataformaSoat.Application.Modulos.SalesParams.Interfaces;
using PlataformaSoat.Application.Modulos.SalesRecon.Interfaces;
using PlataformaSoat.Domain.Interfaces;
using PlataformaSoat.Infrastructure.Cache;
using PlataformaSoat.Infrastructure.Configuration;
using PlataformaSoat.Infrastructure.Persistence;
using PlataformaSoat.Infrastructure.Repositories;
using PlataformaSoat.Infrastructure.Services.Logs;

namespace PlataformaSoat.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core DbContext (Estrategia híbrida: entidades + procedimientos)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetSection("PostgresSettings").Get<PostgresSettings>()?.ConnectionString ?? string.Empty));

        // Repositorios EF Core (para entidades y operaciones CRUD/Unit of Work)
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();

        // Repositorios de Procedimientos Almacenados (PostgreSQL SPs / DTOs)
        services.AddScoped<ISalesParamsRepository, SalesParamsRepository>();
        services.AddScoped<ISalesRepository, SalesRepository>();
        services.AddScoped<ISalesReconRepository, SalesReconRepository>();

        // Servicios de Auditoría y Logging Corporativo (PostgreSQL procedures)
        services.AddScoped<IApiLogService, ApiLogService>();
        services.AddScoped<IErrorLogService, ErrorLogService>();
        services.AddScoped<IEventLogService, EventLogService>();
        services.AddScoped<ITransactionIoLogService, TransactionIoLogService>();

        // Cache
        services.AddMemoryCache();
        services.AddScoped<ICacheService, CacheService>();

        // Seguridad / Tokens
        services.AddScoped<ITokenService, PlataformaSoat.Infrastructure.Services.TokenService>();

        // Generación de Documentos PDF
        services.AddScoped<ISoatReceiptPdfService, PlataformaSoat.Infrastructure.Services.SoatReceiptPdfService>();

        return services;
    }
}
