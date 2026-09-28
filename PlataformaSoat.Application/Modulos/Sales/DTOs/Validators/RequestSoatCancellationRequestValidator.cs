using FluentValidation;
using PlataformaSoat.Application.Modulos.Sales.DTOs.Requests;

namespace PlataformaSoat.Application.Modulos.Sales.DTOs.Validators;

public class RequestSoatCancellationRequestValidator : AbstractValidator<RequestSoatCancellationRequest>
{
    public RequestSoatCancellationRequestValidator()
    {
        RuleFor(x => x.TbSoatPolicyId)
            .GreaterThan(0).WithMessage("El identificador de póliza 'tb_soat_policy_id' debe ser mayor a 0.");

        RuleFor(x => x.CancellationReason)
            .NotEmpty().WithMessage("El motivo de anulación 'cancellation_reason' es obligatorio.");

        RuleFor(x => x.RequestedBy)
            .NotEmpty().WithMessage("El solicitante 'requested_by' es obligatorio.");
    }
}
