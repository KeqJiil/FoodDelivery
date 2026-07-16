using MediatR;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;
using Ordering.Domain.Ids;

namespace Ordering.Application.ChangeOrderLinePrice;

public record ChangeOrderLinePriceCommand(MenuItemRefId MenuItemRefId, Money NewPrice) : IRequest<Result<Error>>;