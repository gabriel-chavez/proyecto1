using System.Threading;
using System.Threading.Tasks;
using PlataformaSoat.Application.Common.DTOs.Logs;

namespace PlataformaSoat.Application.Common.Interfaces;

/// <summary>
/// Servicio para registrar logs de eventos y seguimiento de stack mediante logs.psp_add_event_log.
/// </summary>
public interface IEventLogService
{
    Task<LogOperationResult> AddEventLogAsync(
        AddEventLogRequest request,
        CancellationToken cancellationToken = default);
}
