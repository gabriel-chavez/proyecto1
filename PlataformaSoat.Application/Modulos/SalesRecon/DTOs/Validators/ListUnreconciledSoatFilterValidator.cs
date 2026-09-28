using FluentValidation;
using PlataformaSoat.Application.Modulos.SalesRecon.DTOs.Requests;

namespace PlataformaSoat.Application.Modulos.SalesRecon.DTOs.Validators;

public class ListUnreconciledSoatFilterValidator : AbstractValidator<ListUnreconciledSoatFilter>
{
    public ListUnreconciledSoatFilterValidator()
    {
        RuleFor(x => x.TbBrokerId)
            .GreaterThan(0).WithMessage("El identificador de broker 'tb_broker_id' debe ser mayor a 0.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("La fecha final ('end_date') no puede ser anterior a la fecha inicial ('start_date').");
    }
}
