using MediatR;
using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Ordering.Application.ChangeOrderLinePrice;

public record ChangeOrderLinePriceCommand(MenuItemRefId MenuItemRefId, Currency Currency, decimal Amount)
    : IRequest<Result<Error>>;
