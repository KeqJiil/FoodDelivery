using MediatR;
using Ordering.Application.Abstractions;
using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Ordering.Application.CancelOrder;

public class CancelOrderHandler(IOrderRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CancelOrderCommand, Result<OrderId, Error>>
{
    public async Task<Result<OrderId, Error>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null) return Result<OrderId, Error>.Fail(new Error(ErrorEnum.NotFound, "Order not found"));

        var result = order.Cancel();
        if (!result.IsSuccess)
            return Result<OrderId, Error>.Fail(result.Error ?? new Error(ErrorEnum.Unexpected, "Unexpected error"));

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<OrderId, Error>.Success(order.Id);
    }
}