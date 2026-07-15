using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Abstractions;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Ordering.Application.AddOrderLineItem;

public class AddOrderLineItemHandler(
    IOrderRepository repository,
    IUnitOfWork unitOfWork,
    IMenuPriceForOrderLineAdapter menuPriceForOrderLineAdapter,
    ILogger<AddOrderLineItemHandler> logger)
    : IRequestHandler<AddOrderLineItemCommand, Result<OrderId, Error>>
{
    public async Task<Result<OrderId, Error>> Handle(AddOrderLineItemCommand request,
        CancellationToken cancellationToken)
    {
        var orderLineId = new OrderLineId(Guid.NewGuid());

        var order = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
        {
            logger.LogWarning("Add order line failed: order {OrderId} not found", request.Id);
            return Result<OrderId, Error>.Fail(new Error(ErrorEnum.NotFound, "Order not found"));
        }

        var menuItemPrice = await menuPriceForOrderLineAdapter.GetMenuItemPriceByIdAsync(request.MenuItemRefId,
            cancellationToken);
        if (menuItemPrice is null)
        {
            logger.LogWarning("Add order line failed: menu item {MenuItemRefId} not found", request.MenuItemRefId);
            return Result<OrderId, Error>.Fail(new Error(ErrorEnum.NotFound, "Menu item not found"));
        }

        var result = order.AddOrderLineItem(orderLineId, menuItemPrice, request.MenuItemRefId);
        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to add order line to order {OrderId}: {Error}", order.Id, result.Error);
            return Result<OrderId, Error>.Fail(result.Error ?? new Error(ErrorEnum.Unexpected, "Unexpected error"));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Added order line {OrderLineId} to order {OrderId}", orderLineId, order.Id);

        return Result<OrderId, Error>.Success(request.Id);
    }
}