using Deliveries.Domain.Ids;
using MediatR;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Deliveries.Application.FailDelivery;

public record FailDeliveryCommand(DeliveryId Id, string Reason) : IRequest<Result<Error>>;
