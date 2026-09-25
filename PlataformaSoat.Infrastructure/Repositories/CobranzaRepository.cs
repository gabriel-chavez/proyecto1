using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Modulos.Cobranza.DTOs.Requests;
using PlataformaSoat.Infrastructure.Persistence;
using PlataformaSoat.Application.Modulos.Cobranza.Interfaces;
using Npgsql;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace PlataformaSoat.Infrastructure.Repositories;

public class CobranzaRepository : BaseRepository, ICobranzaRepository
{
    public CobranzaRepository(ApplicationDbContext dbContext,
                               ILogger<CobranzaRepository> logger)
        : base(dbContext, logger) { }

    public async Task<BaseResponse<int>> RevertirCobroAsync(
        RevertirCobroRequest request,
        string codigoSistema,
        CancellationToken cancellationToken = default)
    {
        // TODO: call the stored procedure using EF Core. For now return a placeholder.
        return new BaseResponse<int>
        {
            Resultado = 0,
            Mensaje = "Not implemented",
            Exito = false
        };
    }

    public async Task<BaseResponse<string>> RegisterSoatPolicyAsync(
        RegisterSoatPolicyRequest request,
        string codigoSistema,
        CancellationToken cancellationToken = default)
    {
        var conn = _dbContext.Database.GetDbConnection();
        await using var cmd = conn.CreateCommand();
        // CALL the stored procedure with all IN and INOUT parameters
        cmd.CommandText = "CALL venta.pp_register_soat_policy(@e_client_id, @e_vehicle_id, @e_start_date, @e_end_date, @e_premium, @s_procesado, @s_codigo_error, @s_mensaje, @s_detalle_error, @s_resultado);";
        cmd.CommandType = System.Data.CommandType.Text;

        var pClientId = new Npgsql.NpgsqlParameter("e_client_id", NpgsqlTypes.NpgsqlDbType.Bigint) { Value = request.ClientId };
        var pVehicleId = new Npgsql.NpgsqlParameter("e_vehicle_id", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = request.VehicleId };
        var pStartDate = new Npgsql.NpgsqlParameter("e_start_date", NpgsqlTypes.NpgsqlDbType.Date) { Value = request.StartDate };
        var pEndDate = new Npgsql.NpgsqlParameter("e_end_date", NpgsqlTypes.NpgsqlDbType.Date) { Value = request.EndDate };
        var pPremium = new Npgsql.NpgsqlParameter("e_premium", NpgsqlTypes.NpgsqlDbType.Numeric) { Value = request.Premium };
        // INOUT parameters – start as NULL; they will be populated by the procedure
        var pProcesado = new Npgsql.NpgsqlParameter("s_procesado", NpgsqlTypes.NpgsqlDbType.Boolean) { Direction = System.Data.ParameterDirection.InputOutput, Value = DBNull.Value };
        var pCodigoError = new Npgsql.NpgsqlParameter("s_codigo_error", NpgsqlTypes.NpgsqlDbType.Text) { Direction = System.Data.ParameterDirection.InputOutput, Value = DBNull.Value };
        var pMensaje = new Npgsql.NpgsqlParameter("s_mensaje", NpgsqlTypes.NpgsqlDbType.Text) { Direction = System.Data.ParameterDirection.InputOutput, Value = DBNull.Value };
        var pDetalleError = new Npgsql.NpgsqlParameter("s_detalle_error", NpgsqlTypes.NpgsqlDbType.Text) { Direction = System.Data.ParameterDirection.InputOutput, Value = DBNull.Value };
        var pResultado = new Npgsql.NpgsqlParameter("s_resultado", NpgsqlTypes.NpgsqlDbType.Jsonb) { Direction = System.Data.ParameterDirection.InputOutput, Value = DBNull.Value };

        cmd.Parameters.AddRange(new[] { pClientId, pVehicleId, pStartDate, pEndDate, pPremium, pProcesado, pCodigoError, pMensaje, pDetalleError, pResultado });

        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync(cancellationToken);

        await cmd.ExecuteNonQueryAsync(cancellationToken);

        var response = new BaseResponse<string>
        {
            Exito = pProcesado.Value as bool? ?? false,
            Mensaje = pMensaje.Value?.ToString() ?? string.Empty,
            Resultado = pResultado.Value?.ToString()
        };
        return response;
    }
}
