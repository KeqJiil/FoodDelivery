using Deliveries.Domain.Ids;
using MediatR;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Deliveries.Application.MarkPickedUp;

public record MarkPickedUpCommand(DeliveryId Id) : IRequest<Result<Error>>;
