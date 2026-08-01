using Deliveries.Application.Abstractions;
using Deliveries.Domain.Aggregates;
using Deliveries.Domain.Ids;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Deliveries.Application.CreateDelivery;

public class CreateDeliveryHandler(IDeliveryRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateDeliveryCommand, Result<DeliveryId, Error>>
{
    // SQL Server unique constraint / unique index violation error numbers.
    private static readonly int[] UniqueViolationErrorNumbers = [2601, 2627];

    public async Task<Result<DeliveryId, Error>> Handle(CreateDeliveryCommand request, CancellationToken ct)
    {
        var delivery = Delivery.Create(new DeliveryId(), request.OrderRefId);

        repository.Add(delivery);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: var number } &&
                                            UniqueViolationErrorNumbers.Contains(number))
        {
            return Result<DeliveryId, Error>.Fail(
                Error.Conflict($"A delivery already exists for order {request.OrderRefId.Id}"));
        }

        return Result<DeliveryId, Error>.Success(delivery.Id);
    }
}
