using MediatR;
using OrderRequests.Application.Abstractions;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace OrderRequests.Application.GetOrderRequestById;

public class GetOrderRequestByIdHandler(IOrderRequestReader reader)
    : IRequestHandler<GetOrderRequestByIdQuery, OrderRequestDto?>
{
    public async Task<OrderRequestDto?> Handle(GetOrderRequestByIdQuery request, CancellationToken ct)
    {
        return await reader.GetByIdAsync(request.Id, ct);
    }
}