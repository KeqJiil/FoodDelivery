using MediatR;
using Ordering.Application.Abstractions;
using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Ordering.Application.OrderProcess;

public class OrderProcessHandler(IOrderRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<OrderProcessCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(OrderProcessCommand request, CancellationToken ct)
    {
        var orderId = new OrderId(request.OrderId);
        var order = await repository.GetByIdAsync(orderId, ct);
        if (order is null) return Result<Error>.Fail(Error.NotFound("Not found"));

        var result = order.StartProcessing();
        if (!result.IsSuccess) return Result<Error>.Fail(result.Error!);

        await unitOfWork.SaveChangesAsync(ct);

        return Result<Error>.Success();
    }
}