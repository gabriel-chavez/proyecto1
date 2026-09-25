using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlataformaSoat.Api.Extensions;
using PlataformaSoat.Api.Filters;
using PlataformaSoat.Api.Middleware;
using PlataformaSoat.Application;
using PlataformaSoat.Infrastructure.Configuration;
using PlataformaSoat.Application.Configuration;
using PlataformaSoat.Infrastructure;

using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configuración
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.Configure<PostgresSettings>(builder.Configuration.GetSection("PostgresSettings"));

// Servicios
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddHttpContextAccessor();

// Application
builder.Services.AddApplication();

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Swagger
builder.Services.AddSwaggerDocumentation();

// Health Checks
builder.Services.AddHealthChecks();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Middleware
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
}

// Health Checks
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false // Solo verifica que la app está viva
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => true // Verifica todas las dependencias
});

// CORS
app.UseCors("AllowAll");

// HTTP Pipeline
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
