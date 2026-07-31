using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Payments.Domain.Aggregates;
using Payments.Domain.Enums;
using Payments.Domain.Ids;
using Payments.Infrastructure.Persistence;
using Payments.Infrastructure.Persistence.Readers;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace FoodDelivery.IntegrationTest.Persistence.Payments;

public class PaymentReaderTests(MsSqlContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetByIdAsync_ShouldReturnPayment_WhenItExists()
    {
        var orderRefId = new OrderRefId(Guid.NewGuid());
        var payment = Payment.Create(new PaymentId(Guid.NewGuid()), orderRefId, Money.Create(Currency.Usd, 42m).Ok!);

        await using (var writeContext = CreatePaymentsContext())
        {
            writeContext.Payments.Add(payment);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = CreatePaymentsContext();
        var reader = new PaymentReader(readContext);

        var dto = await reader.GetByIdAsync(payment.Id.Id);

        dto.Should().NotBeNull();
        dto!.OrderRefId.Should().Be(orderRefId.Id);
        dto.Status.Should().Be(PaymentStatus.Pending);
        dto.Amount.Should().Be(Money.Create(Currency.Usd, 42m).Ok);
        dto.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenPaymentDoesNotExist()
    {
        await using var readContext = CreatePaymentsContext();
        var reader = new PaymentReader(readContext);

        var dto = await reader.GetByIdAsync(Guid.NewGuid());

        dto.Should().BeNull();
    }

    private PaymentsDbContext CreatePaymentsContext() =>
        CreateContext<PaymentsDbContext>("payments", o => new PaymentsDbContext(o));
}
