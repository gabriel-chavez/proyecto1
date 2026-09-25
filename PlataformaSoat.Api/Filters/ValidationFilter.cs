using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PlataformaSoat.Application.Common.DTOs;

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
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var errorResponse = new ErrorResponse
            {
                Mensaje = "Uno o más errores de validación ocurrieron.",
                Errores = errors.Values.SelectMany(x => x).ToList()
            };

            var apiResponse = ApiResponse<ErrorResponse>.Failure("Errores de validación", 400);
            apiResponse.Resultado = errorResponse;
            apiResponse.CorrelationId = context.HttpContext.TraceIdentifier;

            context.Result = new BadRequestObjectResult(apiResponse);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
