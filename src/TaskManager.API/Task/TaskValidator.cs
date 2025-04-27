using FluentValidation;
using TaskManager.API.Task.DTOs;

namespace TaskManager.API.Task.Validators
{
    public class TaskModelValidator : AbstractValidator<TaskPostDto>
    {
        public TaskModelValidator()
        {
            RuleFor(task => task.Title)
                .NotEmpty().WithMessage("El título es obligatorio.")
                .MaximumLength(200).WithMessage("El título no puede tener más de 200 caracteres.");

            RuleFor(task => task.Description)
                .MaximumLength(1000).WithMessage("La descripción no puede tener más de 1000 caracteres.")
                .When(task => !string.IsNullOrEmpty(task.Description)); // Solo si existe descripción

            RuleFor(task => task.IsCompleted)
                .NotNull().WithMessage("El estado de completado es obligatorio.");

            //en caso de extencion
            // RuleFor(task => task.Priority)
            //     .InclusiveBetween(0, 2).WithMessage("La prioridad debe ser 0 (Normal), 1 (Alta) o 2 (Urgente).");

            // RuleFor(task => task.DueDate)
            //     .GreaterThanOrEqualTo(DateTime.UtcNow)
            //     .WithMessage("La fecha de vencimiento debe ser futura o hoy.")
            //     .When(task => task.DueDate.HasValue);
        }
    }
}
