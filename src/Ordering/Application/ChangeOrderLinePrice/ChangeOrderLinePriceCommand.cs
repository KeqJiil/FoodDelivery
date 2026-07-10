using MediatR;
using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.Application.ChangeOrderLinePrice;

public record ChangeOrderLinePriceCommand(MenuItemRefId MenuItemRefId, Money NewPrice) : IRequest<Result<Error>>;