using System.Text.Json;
using FluentValidation;
using PlataformaSoat.Application.Modulos.Sales.DTOs.Requests;

namespace PlataformaSoat.Application.Modulos.Sales.DTOs.Validators;

public class RegisterBulkSoatSaleRequestValidator : AbstractValidator<RegisterBulkSoatSaleRequest>
{
    public RegisterBulkSoatSaleRequestValidator()
    {
        RuleFor(x => x.TbBranchId)
            .GreaterThan(0).WithMessage("El campo 'tb_branch_id' debe ser un identificador válido mayor a 0.");

        RuleFor(x => x.TbSalesChannelId)
            .GreaterThan(0).WithMessage("El campo 'tb_sales_channel_id' debe ser un identificador válido mayor a 0.");

        RuleFor(x => x.BusinessName)
            .NotEmpty().WithMessage("La razón social o nombre ('business_name') es obligatorio.");

        RuleFor(x => x.TaxOrIdentityNumber)
            .NotEmpty().WithMessage("El NIT o documento de identidad ('tax_or_identity_number') es obligatorio.");

        RuleFor(x => x.Soats)
            .Must(s => s.ValueKind == JsonValueKind.Array && s.GetArrayLength() > 0)
            .WithMessage("El campo 'soats' debe ser un arreglo JSON con al menos un elemento.");
    }
}
