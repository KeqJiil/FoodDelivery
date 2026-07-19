using OrderRequests.Domain.Aggregates;
using OrderRequests.Domain.Ids;

namespace OrderRequests.Application.Abstractions;

public interface IOrderRequestRepository
{
    public Task<OrderRequest> GetByIdAsync(OrderRequestId id, CancellationToken cancellationToken = default);
    public void Add(OrderRequest orderRequest);
}