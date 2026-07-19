using MediatR;
using OrderRequests.Application.Abstractions;
using OrderRequests.Application.GetOrderRequestById;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace OrderRequests.Application.AcceptOrder;

public class AcceptOrderHandler(IOrderRequestRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<AcceptOrderCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(AcceptOrderCommand request, CancellationToken ct)
    {
        var orderRequest = await repository.GetByIdAsync(request.Id, ct);
        if (orderRequest is null) return Result<Error>.Fail(Error.NotFound("Order Request Not Found"));

        var result = orderRequest.Approve();
        if (!result.IsSuccess) return Result<Error>.Fail(result.Error!);

        await unitOfWork.SaveChangesAsync(ct);

        return Result<Error>.Success();
    }
}