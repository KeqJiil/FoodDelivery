using MediatR;
using Ordering.Application.Abstractions;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Ordering.Application.CreateOrder;

public class CreateOrderHandler(IOrderRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateOrderCommand, Result<OrderId, Error>>
{
    public async Task<Result<OrderId, Error>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderId = new OrderId(Guid.NewGuid());
        var order = Order.Create(orderId, request.RestaurantRefId);

        repository.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<OrderId, Error>.Success(orderId);
    }
}