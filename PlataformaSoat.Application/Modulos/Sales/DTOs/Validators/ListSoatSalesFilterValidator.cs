using FluentValidation;
using PlataformaSoat.Application.Modulos.Sales.DTOs.Requests;

namespace PlataformaSoat.Application.Modulos.Sales.DTOs.Validators;

public class ListSoatSalesFilterValidator : AbstractValidator<ListSoatSalesFilter>
{
    public ListSoatSalesFilterValidator()
    {
        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("La fecha final ('end_date') no puede ser anterior a la fecha inicial ('start_date').");
        });
    }
}
