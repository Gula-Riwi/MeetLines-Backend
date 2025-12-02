using FluentValidation;
using MeetLines.Application.DTOs.Auth;

namespace MeetLines.Application.Validators
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("El token es requerido");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres")
                .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula")
                .Matches(@"[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula")
                .Matches(@"[0-9]").WithMessage("La contraseña debe contener al menos un número")
                .Matches(@"[\W_]").WithMessage("La contraseña debe contener al menos un carácter especial");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("La confirmación de contraseña es requerida")
                .Equal(x => x.NewPassword).WithMessage("Las contraseñas no coinciden");
        }
    }
}