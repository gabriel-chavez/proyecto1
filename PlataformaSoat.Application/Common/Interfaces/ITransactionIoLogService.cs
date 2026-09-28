using System.Threading;
using System.Threading.Tasks;
using PlataformaSoat.Application.Common.DTOs.Logs;

namespace PlataformaSoat.Application.Common.Interfaces;

/// <summary>
/// Servicio para registrar entradas y salidas de procedimientos almacenados mediante logs.psp_add_transaction_io_log.
/// </summary>
public interface ITransactionIoLogService
{
    Task<LogOperationResult> AddTransactionIoLogAsync(
        AddTransactionIoLogRequest request,
        CancellationToken cancellationToken = default);
}
