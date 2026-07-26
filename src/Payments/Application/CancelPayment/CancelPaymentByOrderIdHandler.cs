using MediatR;
using Payments.Application.Abstractions;
using Payments.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Payments.Application.CancelPayment;

public class CancelPaymentByOrderIdHandler(IPaymentRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CancelPaymentByOrderIdCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(CancelPaymentByOrderIdCommand request, CancellationToken ct)
    {
        var id = new OrderRefId(request.OrderId);
        var payment = await repository.GetByOrderIdAsync(id, ct);
        if (payment is null) return Result<Error>.Fail(Error.NotFound("Not found"));

        var result = payment.Cancel();
        if (!result.IsSuccess) return Result<Error>.Fail(result.Error!);

        await unitOfWork.SaveChangesAsync(ct);
        return Result<Error>.Success();
    }
}