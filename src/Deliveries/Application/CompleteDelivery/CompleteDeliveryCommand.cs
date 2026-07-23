using Deliveries.Domain.Ids;
using MediatR;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Deliveries.Application.CompleteDelivery;

public record CompleteDeliveryCommand(DeliveryId Id) : IRequest<Result<Error>>;
