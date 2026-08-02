using MediatR;
using OrderRequests.Application.Abstractions;
using OrderRequests.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace OrderRequests.Application.CancelOrder;

public class CancelOrderHandler(IOrderRequestRepository repository, IUnitOfWork unitOfWork) : IRequestHandler<CancelOrderCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(CancelOrderCommand request, CancellationToken ct)
    {
        var orderRefId = new OrderRefId(request.Id);
        var orderRequest = await repository.GetByOrderRefIdAsync(orderRefId, ct);
        if (orderRequest is null) return Result<Error>.Fail(Error.NotFound("Not found"));

        var result = orderRequest.Cancel();
        if (!result.IsSuccess) return Result<Error>.Fail(result.Error!);
        
        await unitOfWork.SaveChangesAsync(ct);
        
        return Result<Error>.Success();
    }
}