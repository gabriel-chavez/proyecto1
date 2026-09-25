using System.Threading;
using System.Threading.Tasks;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Modulos.Cobranza.DTOs.Requests;

namespace PlataformaSoat.Application.Modulos.Cobranza.Interfaces;

public interface ICobranzaRepository
{
    Task<BaseResponse<int>> RevertirCobroAsync(
        RevertirCobroRequest request,
        string codigoSistema,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<string>> RegisterSoatPolicyAsync(RegisterSoatPolicyRequest request, string codigoSistema, CancellationToken cancellationToken = default);

    }

