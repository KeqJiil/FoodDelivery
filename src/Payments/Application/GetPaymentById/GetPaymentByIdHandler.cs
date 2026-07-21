using MediatR;
using Payments.Application.Abstractions;

namespace Payments.Application.GetPaymentById;

public class GetPaymentByIdHandler(IPaymentReader reader) : IRequestHandler<GetPaymentByIdQuery, PaymentDto?>
{
    public Task<PaymentDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        return reader.GetByIdAsync(request.Id, cancellationToken);
    }
}
