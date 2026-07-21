using SharedKernel.Domain;

namespace Payments.Domain.Ids;

public record PaymentId : TypedId
{
    public PaymentId()
    {
    }

    public PaymentId(Guid id)
        : base(id)
    {
    }
};
