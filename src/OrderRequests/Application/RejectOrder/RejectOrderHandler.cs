using MediatR;
using OrderRequests.Application.Abstractions;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace OrderRequests.Application.RejectOrder;

public class RejectOrderHandler(IOrderRequestRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<RejectOrderCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(RejectOrderCommand request, CancellationToken ct)
    {
        var orderRequest = await repository.GetByIdAsync(request.Id, ct);
        if (orderRequest is null) return Result<Error>.Fail(Error.NotFound("Order Request Not Found"));

        var result = orderRequest.Reject();
        if (!result.IsSuccess) return Result<Error>.Fail(result.Error!);

        await unitOfWork.SaveChangesAsync(ct);

        return Result<Error>.Success();
    }
}