using FluentValidation;
using PlataformaSoat.Application.Modulos.SalesRecon.DTOs.Requests;

namespace PlataformaSoat.Application.Modulos.SalesRecon.DTOs.Validators;

public class ReconcileSoatRequestValidator : AbstractValidator<ReconcileSoatRequest>
{
    public ReconcileSoatRequestValidator()
    {
        RuleFor(x => x.RegisteredBy)
            .NotEmpty().WithMessage("El campo 'registered_by' es obligatorio.");

        RuleFor(x => x.TbReconciliationTypeId)
            .GreaterThan(0).WithMessage("El identificador de tipo de conciliación 'tb_reconciliation_type_id' debe ser mayor a 0.");

        RuleFor(x => x.TbBrokerId)
            .GreaterThan(0).WithMessage("El identificador de broker 'tb_broker_id' debe ser mayor a 0.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("La fecha final ('end_date') no puede ser anterior a la fecha inicial ('start_date').");

        RuleFor(x => x.FinancialInstitution)
            .NotEmpty().WithMessage("La entidad financiera ('financial_institution') es obligatoria.");

        RuleFor(x => x.TransferTransactionCount)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad de transacciones ('transfer_transaction_count') debe ser mayor o igual a 0.");
    }
}
