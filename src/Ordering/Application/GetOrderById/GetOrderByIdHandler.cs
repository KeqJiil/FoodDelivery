using MediatR;
using Ordering.Application.Abstractions;

namespace Ordering.Application.GetOrderById;

public class GetOrderByIdHandler(IOrderReader reader) : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await reader.GetByIdAsync(request.Id, cancellationToken);
        return result;
    }
}