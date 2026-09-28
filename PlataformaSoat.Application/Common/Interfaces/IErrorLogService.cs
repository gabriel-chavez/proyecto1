using System.Threading;
using System.Threading.Tasks;
using PlataformaSoat.Application.Common.DTOs.Logs;

namespace PlataformaSoat.Application.Common.Interfaces;

/// <summary>
/// Servicio para registrar logs de errores por capa o por fallo de base de datos mediante logs.psp_add_error_log.
/// </summary>
public interface IErrorLogService
{
    Task<LogOperationResult> AddErrorLogAsync(
        AddErrorLogRequest request,
        CancellationToken cancellationToken = default);
}
