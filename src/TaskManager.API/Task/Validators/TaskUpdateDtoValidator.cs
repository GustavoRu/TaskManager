using FluentValidation;
using TaskManager.API.Task.DTOs;

namespace TaskManager.API.Task.Validators
{
    public class TaskUpdateDtoValidator : AbstractValidator<TaskUpdateDto>
    {
        public TaskUpdateDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("El título es obligatorio.")
                .MaximumLength(200).WithMessage("El título no puede tener más de 200 caracteres.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("La descripción no puede tener más de 1000 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}
