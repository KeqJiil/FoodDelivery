using OrderRequests.Application.GetOrderRequestById;

namespace OrderRequests.Application.Abstractions;

public interface IOrderRequestReader
{
    public Task<GetOrderRequestByIdRequestDto?> GetByIdAsync(Guid id);
}