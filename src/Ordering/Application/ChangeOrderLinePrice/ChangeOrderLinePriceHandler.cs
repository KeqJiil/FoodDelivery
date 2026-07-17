using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;
using Ordering.Application.Abstractions;
using Ordering.Domain.Aggregates;

namespace Ordering.Application.ChangeOrderLinePrice;

public class ChangeOrderLinePriceHandler(
    IOrderRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<ChangeOrderLinePriceHandler> logger)
    : IRequestHandler<ChangeOrderLinePriceCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(ChangeOrderLinePriceCommand request,
        CancellationToken cancellationToken)
    {
        var moneyResult = Money.Create(request.Currency, request.Amount);
        if (!moneyResult.IsSuccess) return Result<Error>.Fail(moneyResult.Error!);

        var orders =
            await repository.GetByMenuItemIdAsync(request.MenuItemRefId, cancellationToken);
        if (orders.Count == 0) return Result<Error>.Success();

        foreach (var order in orders)
        {
            var result = order.ChangeOrderLinePrice(request.MenuItemRefId, moneyResult.Ok!);
            if (!result.IsSuccess)
                logger.LogWarning("Failed to update price for order {OrderId}: {Error}", order.Id, result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Error>.Success();
    }
}