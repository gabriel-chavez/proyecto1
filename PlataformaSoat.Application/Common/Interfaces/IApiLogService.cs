using System.Threading;
using System.Threading.Tasks;
using PlataformaSoat.Application.Common.DTOs.Logs;

namespace PlataformaSoat.Application.Common.Interfaces;

/// <summary>
/// Servicio para registrar logs de peticiones y respuestas de la API mediante logs.psp_add_api_log.
/// </summary>
public interface IApiLogService
{
    Task<LogOperationResult> AddApiLogAsync(
        AddApiLogRequest request,
        CancellationToken cancellationToken = default);
}
