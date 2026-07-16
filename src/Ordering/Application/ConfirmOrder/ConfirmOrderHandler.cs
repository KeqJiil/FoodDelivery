using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;
using Ordering.Application.Abstractions;
using Ordering.Domain.Ids;

namespace Ordering.Application.ConfirmOrder;

public class ConfirmOrderHandler(
    IOrderRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<ConfirmOrderHandler> logger)
    : IRequestHandler<ConfirmOrderCommand, Result<OrderId, Error>>
{
    public async Task<Result<OrderId, Error>> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
        {
            logger.LogWarning("Confirm failed: order {OrderId} not found", request.Id);
            return Result<OrderId, Error>.Fail(new Error(ErrorEnum.NotFound, "Order not found"));
        }

        var result = order.Confirm();
        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to confirm order {OrderId}: {Error}", order.Id, result.Error);
            return Result<OrderId, Error>.Fail(result.Error ?? new Error(ErrorEnum.Unexpected, "Unexpected error"));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Confirmed order {OrderId}", order.Id);

        return Result<OrderId, Error>.Success(order.Id);
    }
}