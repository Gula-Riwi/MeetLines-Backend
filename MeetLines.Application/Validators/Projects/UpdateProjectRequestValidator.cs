using FluentValidation;
using MeetLines.Application.DTOs.Projects;
using MeetLines.Domain.ValueObjects;

namespace MeetLines.Application.Validators.Projects
{
    /// <summary>
    /// Validador para actualizar un proyecto
    /// </summary>
    public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
    {
        public UpdateProjectRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("El nombre del proyecto es requerido")
                .MinimumLength(2)
                .WithMessage("El nombre debe tener al menos 2 caracteres")
                .MaximumLength(100)
                .WithMessage("El nombre no puede exceder 100 caracteres");
            
            RuleFor(x => x.Subdomain)
                .Cascade(CascadeMode.Stop)
                .Must(sd => string.IsNullOrWhiteSpace(sd) || SubdomainValidator.IsValid(sd, out _))
                .WithMessage("El subdominio no es válido. Debe tener entre 3 y 63 caracteres, sólo minúsculas, números y guiones, no comenzar ni terminar con guión y no usar reservados.")
                .When(x => x.Subdomain != null && x.Subdomain != "");
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
