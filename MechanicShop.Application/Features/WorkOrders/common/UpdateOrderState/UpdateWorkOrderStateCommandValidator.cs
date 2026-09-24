using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.common.UpdateOrderState;

public sealed class UpdateWorkOrderStateCommandValidator : AbstractValidator<UpdateWorkOrderStateCommand>
{
    public UpdateWorkOrderStateCommandValidator()
    {
        RuleFor(x => x.State)
           .IsInEnum()
           .WithErrorCode("WorkOrderStatus_Invalid")
           .WithMessage("Status must be a valid WorkOrderStatus value.");
    }
}