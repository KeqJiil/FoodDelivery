using Deliveries.Domain.Ids;
using MediatR;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Deliveries.Application.CreateDelivery;

public record CreateDeliveryCommand(OrderRefId OrderRefId) : IRequest<Result<DeliveryId, Error>>;
