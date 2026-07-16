using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;
using Ordering.Application.Abstractions;
using Ordering.Domain.Ids;

namespace Ordering.Application.CancelOrder;

public class CancelOrderHandler(IOrderRepository repository, IUnitOfWork unitOfWork, ILogger<CancelOrderHandler> logger)
    : IRequestHandler<CancelOrderCommand, Result<OrderId, Error>>
{
    public async Task<Result<OrderId, Error>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
        {
            logger.LogWarning("Cancel failed: order {OrderId} not found", request.Id);
            return Result<OrderId, Error>.Fail(new Error(ErrorEnum.NotFound, "Order not found"));
        }

        var result = order.Cancel();
        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to cancel order {OrderId}: {Error}", order.Id, result.Error);
            return Result<OrderId, Error>.Fail(result.Error ?? new Error(ErrorEnum.Unexpected, "Unexpected error"));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Cancelled order {OrderId}", order.Id);

        return Result<OrderId, Error>.Success(order.Id);
    }
}