using FluentValidation;
using MeetLines.Application.DTOs.Profile;

namespace MeetLines.Application.Validators
{
    public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
    {
        public UpdateProfileRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

            RuleFor(x => x.Phone)
                .MaximumLength(20).WithMessage("El teléfono no puede exceder 20 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.Timezone)
                .NotEmpty().WithMessage("La zona horaria es requerida")
                .MaximumLength(50).WithMessage("La zona horaria no puede exceder 50 caracteres");
        }
    }
}