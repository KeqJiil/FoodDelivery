using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Abstractions;
using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Ordering.Application.RemoveOrderLineItem;

public class RemoveOrderLineItemHandler(
    IOrderRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<RemoveOrderLineItemHandler> logger)
    : IRequestHandler<RemoveOrderLineItemCommand, Result<OrderId, Error>>
{
    public async Task<Result<OrderId, Error>> Handle(RemoveOrderLineItemCommand request,
        CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
        {
            logger.LogWarning("Remove order line failed: order {OrderId} not found", request.Id);
            return Result<OrderId, Error>.Fail(new Error(ErrorEnum.NotFound, "Order not found"));
        }

        var result = order.RemoveOrderLineItem(request.OrderLineId);
        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to remove order line {OrderLineId} from order {OrderId}: {Error}",
                request.OrderLineId, request.Id, result.Error);
            return Result<OrderId, Error>.Fail(result.Error ?? new Error(ErrorEnum.Unexpected, "Unexpected Error"));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Removed order line {OrderLineId} from order {OrderId}", request.OrderLineId,
            request.Id);

        return Result<OrderId, Error>.Success(request.Id);
    }
}