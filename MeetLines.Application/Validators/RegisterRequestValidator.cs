using FluentValidation;
using MeetLines.Application.DTOs.Auth;

namespace MeetLines.Application.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El email no es válido")
                .MaximumLength(255).WithMessage("El email no puede exceder 255 caracteres");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres")
                .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula")
                .Matches(@"[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula")
                .Matches(@"[0-9]").WithMessage("La contraseña debe contener al menos un número")
                .Matches(@"[\W_]").WithMessage("La contraseña debe contener al menos un carácter especial");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("La confirmación de contraseña es requerida")
                .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden");

            RuleFor(x => x.Phone)
                .MaximumLength(20).WithMessage("El teléfono no puede exceder 20 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.Timezone)
                .NotEmpty().WithMessage("La zona horaria es requerida");
        }
    }
}