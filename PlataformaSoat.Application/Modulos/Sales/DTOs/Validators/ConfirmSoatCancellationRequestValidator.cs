using FluentValidation;
using PlataformaSoat.Application.Modulos.Sales.DTOs.Requests;

namespace PlataformaSoat.Application.Modulos.Sales.DTOs.Validators;

public class ConfirmSoatCancellationRequestValidator : AbstractValidator<ConfirmSoatCancellationRequest>
{
    public ConfirmSoatCancellationRequestValidator()
    {
        RuleFor(x => x.TbSoatCancellationId)
            .GreaterThan(0).WithMessage("El identificador de solicitud 'tb_soat_cancellation_id' debe ser mayor a 0.");

        RuleFor(x => x.ConfirmedBy)
            .NotEmpty().WithMessage("El usuario confirmador 'confirmed_by' es obligatorio.");
    }
}
