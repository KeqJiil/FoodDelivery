using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using Ordering.Application.Abstractions;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Ids;

namespace Ordering.Application.CreateOrder;

public class CreateOrderHandler(IOrderRepository repository, IUnitOfWork unitOfWork, ILogger<CreateOrderHandler> logger)
    : IRequestHandler<CreateOrderCommand, Result<OrderId, Error>>
{
    public async Task<Result<OrderId, Error>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderId = new OrderId();
        var order = Order.Create(orderId, request.RestaurantRefId);

        repository.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created order {OrderId}", orderId);

        return Result<OrderId, Error>.Success(orderId);
    }
}