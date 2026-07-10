using MediatR;
using Ordering.Application.Abstractions;
using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Ordering.Application.ConfirmOrder;

public class ConfirmOrderHandler(IOrderRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<ConfirmOrderCommand, Result<OrderId, Error>>
{
    public async Task<Result<OrderId, Error>> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null) return Result<OrderId, Error>.Fail(new Error(ErrorEnum.NotFound, "Order not found"));

        var result = order.Confirm();
        if (!result.IsSuccess)
            return Result<OrderId, Error>.Fail(result.Error ?? new Error(ErrorEnum.Unexpected, "Unexpected error"));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<OrderId, Error>.Success(order.Id);
    }
}