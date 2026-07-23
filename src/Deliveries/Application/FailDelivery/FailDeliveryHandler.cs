using Deliveries.Application.Abstractions;
using MediatR;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Deliveries.Application.FailDelivery;

public class FailDeliveryHandler(IDeliveryRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<FailDeliveryCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(FailDeliveryCommand request, CancellationToken ct)
    {
        var delivery = await repository.GetByIdAsync(request.Id, ct);
        if (delivery is null) return Result<Error>.Fail(Error.NotFound("Delivery Not Found"));

        var result = delivery.Fail(request.Reason);
        if (!result.IsSuccess) return Result<Error>.Fail(result.Error!);

        await unitOfWork.SaveChangesAsync(ct);

        return Result<Error>.Success();
    }
}
