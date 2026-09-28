using FluentValidation;
using PlataformaSoat.Application.Modulos.SalesRecon.DTOs.Requests;

namespace PlataformaSoat.Application.Modulos.SalesRecon.DTOs.Validators;

public class CancelReconciliationRequestValidator : AbstractValidator<CancelReconciliationRequest>
{
    public CancelReconciliationRequestValidator()
    {
        RuleFor(x => x.TbReconciliationId)
            .GreaterThan(0).WithMessage("El identificador de conciliación 'tb_reconciliation_id' debe ser mayor a 0.");

        RuleFor(x => x.CancelledBy)
            .NotEmpty().WithMessage("El campo 'cancelled_by' es obligatorio.");

        RuleFor(x => x.CancellationReason)
            .NotEmpty().WithMessage("El motivo de anulación 'cancellation_reason' es obligatorio.");
    }
}
