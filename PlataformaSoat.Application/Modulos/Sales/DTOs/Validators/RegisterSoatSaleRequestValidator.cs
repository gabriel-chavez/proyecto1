using FluentValidation;
using PlataformaSoat.Application.Modulos.Sales.DTOs.Requests;

namespace PlataformaSoat.Application.Modulos.Sales.DTOs.Validators;

public class RegisterSoatSaleRequestValidator : AbstractValidator<RegisterSoatSaleRequest>
{
    public RegisterSoatSaleRequestValidator()
    {
        RuleFor(x => x.RegisteredBy)
            .NotEmpty().WithMessage("El campo 'registered_by' es obligatorio.");

        RuleFor(x => x.TbBranchId)
            .GreaterThan(0).WithMessage("El campo 'tb_branch_id' debe ser mayor a 0.");

        RuleFor(x => x.TbBrokerId)
            .GreaterThan(0).WithMessage("El campo 'tb_broker_id' debe ser mayor a 0.");

        RuleFor(x => x.TbCommercializerId)
            .GreaterThan(0).WithMessage("El campo 'tb_commercializer_id' debe ser mayor a 0.");

        RuleFor(x => x.TbSalesChannelId)
            .GreaterThan(0).WithMessage("El campo 'tb_sales_channel_id' debe ser mayor a 0.");

        RuleFor(x => x.BusinessName)
            .NotEmpty().WithMessage("El campo 'business_name' (razón social o nombre) es obligatorio.");

        RuleFor(x => x.TaxOrIdentityNumber)
            .NotEmpty().WithMessage("El campo 'tax_or_identity_number' (NIT o documento de identidad) es obligatorio.");

        RuleFor(x => x.Cufd)
            .NotEmpty().WithMessage("El código único de facturación diaria 'cufd' es obligatorio.");

        RuleFor(x => x.InvoiceNumber)
            .GreaterThan(0).WithMessage("El número de factura 'invoice_number' debe ser mayor a 0.");

        RuleFor(x => x.TbDepartmentId)
            .GreaterThan(0).WithMessage("El identificador de departamento 'tb_department_id' debe ser mayor a 0.");

        RuleFor(x => x.TbPolicyYearId)
            .GreaterThan(0).WithMessage("El identificador de gestión 'tb_policy_year_id' debe ser mayor a 0.");

        RuleFor(x => x.TbUsageId)
            .GreaterThan(0).WithMessage("El identificador de uso de vehículo 'tb_usage_id' debe ser mayor a 0.");

        RuleFor(x => x.TbVehicleTypeId)
            .GreaterThan(0).WithMessage("El identificador de tipo de vehículo 'tb_vehicle_type_id' debe ser mayor a 0.");

        RuleFor(x => x.TbPlateTypeId)
            .GreaterThan(0).WithMessage("El identificador de tipo de placa 'tb_plate_type_id' debe ser mayor a 0.");

        RuleFor(x => x.PaidPremium)
            .GreaterThan(0).WithMessage("La prima pagada 'paid_premium' debe ser mayor a 0.");

        RuleFor(x => x.PlateOrChassis)
            .NotEmpty().WithMessage("El número de placa o chasis 'plate_or_chassis' es obligatorio.");

        RuleFor(x => x.Policyholder)
            .NotEmpty().WithMessage("El nombre del asegurado o tomador 'policyholder' es obligatorio.");
    }
}
