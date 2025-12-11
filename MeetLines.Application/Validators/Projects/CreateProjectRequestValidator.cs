using FluentValidation;
using MeetLines.Application.DTOs.Projects;

namespace MeetLines.Application.Validators.Projects
{
    /// <summary>
    /// Validador para crear un nuevo proyecto
    /// </summary>
    public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
    {
        public CreateProjectRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("El nombre del proyecto es requerido")
                .MinimumLength(2)
                .WithMessage("El nombre debe tener al menos 2 caracteres")
                .MaximumLength(100)
                .WithMessage("El nombre no puede exceder 100 caracteres");

            RuleFor(x => x.Industry)
                .MaximumLength(50)
                .WithMessage("La industria no puede exceder 50 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Industry));

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("La descripción no puede exceder 500 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Description));


        }
    }
}
