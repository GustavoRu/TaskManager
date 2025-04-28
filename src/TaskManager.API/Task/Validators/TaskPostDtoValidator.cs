using FluentValidation;
using TaskManager.API.Task.DTOs;

namespace TaskManager.API.Task.Validators
{
    public class TaskPostDtoValidator : AbstractValidator<TaskPostDto>
    {
        public TaskPostDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("El título es obligatorio.")
                .MaximumLength(200).WithMessage("El título no puede tener más de 200 caracteres.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("La descripción es obligatoria.")
                .MaximumLength(1000).WithMessage("La descripción no puede tener más de 1000 caracteres.");
        }
    }
}
