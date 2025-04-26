using FluentValidation;
using TaskManager.API.Auth.DTOs;
namespace TaskManager.API.Auth
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre es obligatorio");
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Correo electrónico inválido");
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres");
        }
    }

    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Correo electrónico inválido");
            RuleFor(x => x.Password).NotEmpty().WithMessage("La contraseña es obligatoria");
        }
    }
}