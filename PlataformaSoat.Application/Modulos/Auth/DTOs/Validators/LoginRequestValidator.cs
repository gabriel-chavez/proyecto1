using FluentValidation;
using PlataformaSoat.Application.Modulos.Auth.DTOs;

namespace PlataformaSoat.Application.Modulos.Auth.DTOs.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.");
    }
}
