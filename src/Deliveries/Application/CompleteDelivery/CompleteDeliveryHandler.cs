using Deliveries.Application.Abstractions;
using MediatR;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Deliveries.Application.CompleteDelivery;

public class CompleteDeliveryHandler(IDeliveryRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CompleteDeliveryCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(CompleteDeliveryCommand request, CancellationToken ct)
    {
        var delivery = await repository.GetByIdAsync(request.Id, ct);
        if (delivery is null) return Result<Error>.Fail(Error.NotFound("Delivery Not Found"));

        var result = delivery.Complete();
        if (!result.IsSuccess) return Result<Error>.Fail(result.Error!);

        await unitOfWork.SaveChangesAsync(ct);

        return Result<Error>.Success();
    }
}
