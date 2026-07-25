using MediatR;
using Ordering.Application.Abstractions;
using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Ordering.Application.OrderFail;

public class OrderFailHandler(IOrderRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<OrderFailCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(OrderFailCommand request, CancellationToken ct)
    {
        var orderId = new OrderId(request.OrderId);
        var order = await repository.GetByIdAsync(orderId, ct);
        if (order is null) return Result<Error>.Fail(Error.NotFound("Not found"));

        var result = order.Fail();
        if (!result.IsSuccess) return Result<Error>.Fail(result.Error!);

        await unitOfWork.SaveChangesAsync(ct);

        return Result<Error>.Success();
    }
}