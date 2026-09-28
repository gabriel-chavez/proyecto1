using FluentValidation;
using PlataformaSoat.Application.Modulos.SalesParams.DTOs.Requests;

namespace PlataformaSoat.Application.Modulos.SalesParams.DTOs.Validators;

public class CalculatePremiumRequestValidator : AbstractValidator<CalculatePremiumRequest>
{
    public CalculatePremiumRequestValidator()
    {
        RuleFor(x => x.TbPolicyYearId)
            .GreaterThan(0).WithMessage("El identificador de gestión 'tb_policy_year_id' debe ser mayor a 0.");

        RuleFor(x => x.TbUsageId)
            .GreaterThan(0).WithMessage("El identificador de uso 'tb_usage_id' debe ser mayor a 0.");

        RuleFor(x => x.TbVehicleTypeId)
            .GreaterThan(0).WithMessage("El identificador de tipo de vehículo 'tb_vehicle_type_id' debe ser mayor a 0.");

        RuleFor(x => x.TbDepartmentId)
            .GreaterThan(0).WithMessage("El identificador de departamento 'tb_department_id' debe ser mayor a 0.");
    }
}
