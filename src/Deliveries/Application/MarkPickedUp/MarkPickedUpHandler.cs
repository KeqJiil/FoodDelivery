using Deliveries.Application.Abstractions;
using MediatR;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Deliveries.Application.MarkPickedUp;

public class MarkPickedUpHandler(IDeliveryRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<MarkPickedUpCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(MarkPickedUpCommand request, CancellationToken ct)
    {
        var delivery = await repository.GetByIdAsync(request.Id, ct);
        if (delivery is null) return Result<Error>.Fail(Error.NotFound("Delivery Not Found"));

        var result = delivery.MarkPickedUp();
        if (!result.IsSuccess) return Result<Error>.Fail(result.Error!);

        await unitOfWork.SaveChangesAsync(ct);

        return Result<Error>.Success();
    }
}
