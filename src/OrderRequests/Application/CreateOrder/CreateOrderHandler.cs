using MediatR;
using OrderRequests.Application.Abstractions;
using OrderRequests.Domain.Aggregates;
using OrderRequests.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace OrderRequests.Application.CreateOrder;

public class CreateOrderHandler(IOrderRequestRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateOrderCommand, Result<OrderRequestId, Error>>
{
    public async Task<Result<OrderRequestId, Error>> Handle(CreateOrderCommand request,
        CancellationToken ct)
    {
        var orderRequestId = new OrderRequestId();
        var orderRequest = OrderRequest.Create(orderRequestId, request.OrderRefId, request.RestaurantRefId);

        repository.Add(orderRequest);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<OrderRequestId, Error>.Success(orderRequestId);
    }
}