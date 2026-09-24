using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.common.CreateWorkORDER
{
    public class CreateWorkOrderCommandValidator : AbstractValidator<CreateWorkOrderCommand>
    {
        public CreateWorkOrderCommandValidator()
        {
            RuleFor(x => x.VehicleId).NotEmpty().WithMessage("VehicleId is required.");
            RuleFor(x => x.StartAt).GreaterThanOrEqualTo(DateTimeOffset.UtcNow).WithMessage("StartAt must be a future date.");
            RuleFor(x => x.RepairTaskIds).NotEmpty().WithMessage("RepairTaskIds are required.");
            RuleFor(x => x.Spot).IsInEnum().WithMessage("Spot must be a valid enum value.");
            RuleFor(x => x.LaborId).NotEmpty().When(x => x.LaborId.HasValue).WithMessage("LaborId must be a valid GUID if provided.");
        }
    }
}
