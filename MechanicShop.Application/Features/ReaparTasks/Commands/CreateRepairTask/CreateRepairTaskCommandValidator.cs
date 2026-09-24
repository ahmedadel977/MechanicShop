using FluentValidation;

namespace MechanicShop.Application.Features.ReaparTasks.Commands.CreateRepairTask
{
    public class CreateRepairTaskCommandValidator : AbstractValidator<CreateRepairTaskCommand>
    {
        public CreateRepairTaskCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
            RuleFor(x => x.LaborCost)
                .GreaterThanOrEqualTo(0).WithMessage("Labor cost must be greater than or equal to 0.");
            RuleFor(x => x.Parts)
                .NotNull().WithMessage("Parts list cannot be null.")
                .Must(parts => parts.Count > 0).WithMessage("Parts list must contain at least one part.");
            RuleForEach(x => x.Parts)
                .SetValidator(new CreateRepairTaskPartCommandValidator());

            RuleFor(x => x.EstimatedDurationInMin)
                .IsInEnum().WithMessage("Estimated duration must be a valid enum value.");
        }
    }
}
