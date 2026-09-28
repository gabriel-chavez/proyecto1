using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Common.DTOs.Logs;
using PlataformaSoat.Application.Common.Interfaces;

namespace PlataformaSoat.Api.Filters;

public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => JsonNamingPolicy.SnakeCaseLower.ConvertName(kvp.Key.TrimStart('$', '.')),
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var errorMessages = errors.Values.SelectMany(x => x).ToList();
            var combinedError = string.Join("; ", errorMessages);

            var response = new BaseResponse<Dictionary<string, string[]>>
            {
                Response = errors,
                Success = false,
                StatusMessage = "VALIDATION_ERROR",
                ResponseMessage = "Uno o más errores de validación ocurrieron.",
                ErrorMessage = combinedError
            };

            // Registrar error de validación en la capa API
            var errorLogService = context.HttpContext.RequestServices.GetService<IErrorLogService>();
            if (errorLogService != null)
            {
                _ = errorLogService.AddErrorLogAsync(new AddErrorLogRequest
                {
                    Layer = "API",
                    Severity = "WARNING",
                    ErrorCode = "VALIDATION_ERROR",
                    ErrorType = "ModelStateValidation",
                    Message = combinedError,
                    Origin = JsonSerializer.Serialize(new { endpoint = context.HttpContext.Request.Path.Value }),
                    Context = JsonSerializer.Serialize(new { trace_id = context.HttpContext.TraceIdentifier, errors = errors })
                });
            }

            context.Result = new BadRequestObjectResult(response);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
