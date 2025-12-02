using FluentValidation;
using MeetLines.Application.DTOs.Auth;

namespace MeetLines.Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El email no es válido");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida");
        }
    }
}