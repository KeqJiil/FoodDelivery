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
    private const int BatchSize = 500;

    public async Task<Result<Error>> Handle(ChangeOrderLinePriceCommand request,
        CancellationToken cancellationToken)
    {
        var moneyResult = Money.Create(request.Currency, request.Amount);
        if (!moneyResult.IsSuccess) return Result<Error>.Fail(moneyResult.Error!);
        
        IReadOnlyList<OrderId> batch;
        OrderId? lastId = null;

        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            batch = await repository.GetDraftIdsByMenuItemIdAsync(request.MenuItemRefId, BatchSize, lastId,
                cancellationToken);
            if (batch.Count == 0) break;
            lastId = batch[^1];

            var orders = await repository.GetByIdsAsync(batch, cancellationToken);
            foreach (var order in orders)
            {
                var result = order.ChangeOrderLinePrice(request.MenuItemRefId, moneyResult.Ok!);
                if (!result.IsSuccess)
                    logger.LogWarning("Failed to update price for order {OrderId}: {Error}", order.Id, result.Error);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            unitOfWork.ClearChangeTracker();
        } while (batch.Count == BatchSize);

        return Result<Error>.Success();
    }
}
