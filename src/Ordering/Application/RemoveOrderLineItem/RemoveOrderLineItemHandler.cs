using MediatR;
using Ordering.Application.Abstractions;
using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Ordering.Application.RemoveOrderLineItem;

public class RemoveOrderLineItemHandler(IOrderRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveOrderLineItemCommand, Result<OrderId, Error>>
{
    public async Task<Result<OrderId, Error>> Handle(RemoveOrderLineItemCommand request,
        CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
            return Result<OrderId, Error>.Fail(new Error(ErrorEnum.NotFound, "Order not found"));

        var result = order.RemoveOrderLineItem(request.OrderLineId);
        if (!result.IsSuccess)
            return Result<OrderId, Error>.Fail(result.Error ?? new Error(ErrorEnum.Unexpected, "Unexpected Error"));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<OrderId, Error>.Success(request.Id);
    }
}