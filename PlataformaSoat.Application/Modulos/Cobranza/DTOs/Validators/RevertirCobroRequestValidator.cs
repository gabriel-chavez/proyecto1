using FluentValidation;
using PlataformaSoat.Application.Modulos.Cobranza.DTOs.Requests;

namespace PlataformaSoat.Application.Modulos.Cobranza.DTOs.Validators;

public class RevertirCobroRequestValidator : AbstractValidator<RevertirCobroRequest>
{
    public RevertirCobroRequestValidator()
    {
        RuleFor(x => x.UsuarioAut)
            .NotEmpty().WithMessage("El usuario es requerido.");

        RuleFor(x => x.SeguridadToken)
            .GreaterThan(0).WithMessage("El token de seguridad es requerido.");

        RuleFor(x => x.TPlanPagoDetalleFk)
            .GreaterThan(0).WithMessage("El detalle de plan de pago es requerido.");

        RuleFor(x => x.Motivo)
            .NotEmpty().WithMessage("El motivo es requerido.");
    }
}
